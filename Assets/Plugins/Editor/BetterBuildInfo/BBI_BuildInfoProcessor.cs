// Copyright (c) 2016 Piotr Gwiazdowski. All rights reserved.
// This file is a part of Better Build Info project.
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO;
using System.Collections;
using System.Text;

using Stopwatch = System.Diagnostics.Stopwatch;
using Better.BuildInfo.Internal;
using UnitySprite = UnityEngine.Sprite;

namespace Better.BuildInfo
{
    [InitializeOnLoad]
    public sealed partial class BuildInfoProcessor : IDisposable
    {
        private class Config
        {
            public bool testMode;
            public string[] expectedScenesNames = null;
            public string dataDirectoryOverride;
        }

        private static Config s_queuedConfig;
        private static BuildInfoProcessor s_buildSession = null;

        private Config config;
        private Stopwatch buildTimer = new Stopwatch();
        private Stopwatch toolOverheadTimer = new Stopwatch();

        private Dictionary<string, AssetInfo> assetsUsedByScenes = new Dictionary<string, AssetInfo>();
        private List<string> processedScenes = new List<string>();
        private List<string> processedScenesNonAltered = new List<string>();
        private List<AssetProperty[]> scenesDetails = new List<AssetProperty[]>();



        private bool deferAnalysis = false;
        private BuildTarget buildTarget;
        private string buildPath;

        private BuildInfoAssetDetailsCollector detailsCollector = null;


        public BuildInfoProcessor()
        {
            config = s_queuedConfig ?? new Config();
            s_queuedConfig = null;
        }

        public void Dispose()
        {
            buildTimer.Stop();
            toolOverheadTimer.Stop();
            if (detailsCollector != null)
                detailsCollector.Dispose();
        }

        static BuildInfoProcessor()
        {


            EditorApplication.update += () =>
            {
                string pathToOpen = BuildInfoSettings.ReportToOpen;
                if (!string.IsNullOrEmpty(pathToOpen))
                {
                    BuildInfoSettings.ReportToOpen = null;

                    Log.Info("Opening report: {0}", pathToOpen);

                    var wnd = BuildInfoWindow.GetWindow<BuildInfoWindow>();
                    wnd.Show();
                    wnd.OpenFile(pathToOpen);
                }

                if (s_buildSession != null)
                {
                    OnBuildEnded();
                }
            };
        }

        internal static void OnBuildEnded()
        {
            if (s_buildSession == null)
                throw new System.InvalidOperationException("Build was not in progress");

            try
            {
                using (s_buildSession)
                {
                    if (s_buildSession.deferAnalysis)
                    {
                        s_buildSession.PostProcessBuild(s_buildSession.buildTarget, s_buildSession.buildPath);
                    }
                    else if (s_buildSession.buildTimer.IsRunning)
                    {
                        Log.Warning("The build seems to have failed or been interrupted");
                    }
                }
            }
            finally
            {
                s_buildSession = null;
            }
        }

        private static string EditorLogPath
        {
            get
            {
                // first try to get it from the command line
                var args = System.Environment.GetCommandLineArgs();
                var logFileSwitchIndex = Array.FindIndex(args, x => string.Equals(x, "-logFile", StringComparison.OrdinalIgnoreCase));
                if (logFileSwitchIndex >= 0 && logFileSwitchIndex < args.Length - 1)
                {
                    return args[logFileSwitchIndex + 1];
                }

                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32NT:
                        return Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Unity\Editor\Editor.log");
                    case PlatformID.MacOSX:
                    case PlatformID.Unix:
                        return ReliablePath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Logs/Unity/Editor.log");
                    default:
                        throw new NotSupportedException("Platform " + Environment.OSVersion.Platform + " not supported");
                }
            }
        }

        private static Config QueuedConfig
        {
            get { return s_queuedConfig = (s_queuedConfig ?? new Config()); }
        }

        private static void WarnIfDisabled()
        {
            if (!BetterBuildInfo.IsEnabled)
            {
                Log.Warning("Better Build Info is not enabled. This operation will have no effect.");
            }
        }

        public static void SetTestMode(bool value)
        {
            WarnIfDisabled();
            QueuedConfig.testMode = value;
        }

        public static void SetExpectedScenesPaths(string[] paths)
        {
            WarnIfDisabled();
            QueuedConfig.expectedScenesNames = paths;
        }

        public static void SetDataDirectoryOverride(string dataDirectoryPath)
        {
            WarnIfDisabled();
            QueuedConfig.dataDirectoryOverride = dataDirectoryPath;
        }

        [UnityEditor.Callbacks.PostProcessScene(int.MaxValue)]
        public static void OnPostprocessScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (!BetterBuildInfo.IsEnabled)
            {
                return;
            }

            if (s_buildSession == null)
            {
                s_buildSession = new BuildInfoProcessor();
            }

            s_buildSession.PostProcessScene();
        }

        

        [UnityEditor.Callbacks.PostProcessBuild(int.MaxValue)]
        public static void OnPostprocessBuild(BuildTarget target, string path)
        {
            if (!BetterBuildInfo.IsEnabled)
            {
                return;
            }

            if (s_buildSession == null)
            {
                Log.Error("Somehow the tool hasn't recognized that the build is in progress");
                return;
            }

            if (UnityVersionAgnostic.AssetLogPrintedAfterPostProcessors)
            {
                Log.Debug("This Unity version prints assets usage *after* post processors are run (possibly a bug); deferring the analysis to the first editor update after the build.");
                s_buildSession.buildTarget = target;
                s_buildSession.buildPath = path;
                s_buildSession.deferAnalysis = true;
            }
            else
            {
                try
                {
                    using (s_buildSession)
                    {
                        s_buildSession.PostProcessBuild(target, path);
                    }
                }
                finally
                {
                    s_buildSession = null;
                }
            }

            return;
        }

        private void PostProcessScene()
        {
            if (!buildTimer.IsRunning)
            {
                buildTimer.Start();
                BuildInfoSettings.ReportToOpen = null;
            }

            if (BuildInfoSettings.Instance.collectAssetsDetails && detailsCollector == null)
            {
                bool checkCompressedSize = false;
                if (UnityVersionAgnostic.IsGetRawTextureDataSupported)
                {
                    checkCompressedSize = BuildInfoSettings.Instance.checkAssetsCompressedSize;
                }
                detailsCollector = new BuildInfoAssetDetailsCollector(checkCompressedSize);
            }

            try
            {
                toolOverheadTimer.Start();

                var sceneIndex = processedScenes.Count;

                string scenePath = UnityVersionAgnostic.CurrentScene;

                if (processedScenesNonAltered.Contains(scenePath))
                {
                    Log.Debug("Scene {0} already processed, ignoring", scenePath);
                }
                else
                {
                    if (config.expectedScenesNames != null)
                    {
                        if (config.expectedScenesNames.Length > sceneIndex)
                        {
                            scenePath = config.expectedScenesNames[sceneIndex];
                        }
                        else
                        {
                            Log.Warning("SetExpectedScenesPaths was not provided with enough paths for the build (current scene no: {0} ({1})). " +
                                       "Carrying on, but bad things may happen.", sceneIndex, scenePath);
                        }
                    }
                    else if (string.IsNullOrEmpty(scenePath) || scenePath.ToLower().StartsWith("temp/"))
                    {
                        // oopsie
                        var guessName = EditorBuildSettings.scenes.Where(x => x.enabled).Select(x => x.path).Skip(sceneIndex).FirstOrDefault();
                        if (guessName != null)
                        {
                            Log.Warning("Detected a temp scene ({0}), guessed it's really {1} based on editor build settings.\n" +
                                       "This happens if a scene to be included in a build is opened. If you call BuildPlayer method from script " +
                                       "consider calling Better.BuildInfoProcessor.SetExpectedScenesPaths first.", scenePath, guessName);

                            scenePath = guessName;
                        }
                        else
                        {
                            Log.Warning("Detected a temp scene ({0}), but unable to guess it's real name.\n" +
                                       "This happens if a scene to be included in a build is opened and you are using BuildPlayer method from script; " +
                                       "consider calling Better.BuildInfoProcessor.SetExpectedScenesPaths first."
                                       , guessName);
                        }
                    }

                    AssetProperty[] details;
                    CollectUsedAssets(scenePath, assetsUsedByScenes, detailsCollector, out details);
                    processedScenes.Add(scenePath);
                    scenesDetails.Add(details);
                    processedScenesNonAltered.Add(UnityVersionAgnostic.CurrentScene);
                }
            }
            finally
            {
                toolOverheadTimer.Stop();
            }
        }

        private void PostProcessBuild(BuildTarget target, string path)
        {
            toolOverheadTimer.Start();

            try
            {
                EditorUtility.DisplayProgressBar("Better Build Info", "Analyzing build...", 1.0f);

                // now analyze the build log
                var editorLogPath = EditorLogPath;
                Log.Debug("Using editor log at: {0}", editorLogPath);

                Dictionary<string, long> scenesSizes = new Dictionary<string, long>();

                var infos = GetAssetsAndSizesFromBuildLog(editorLogPath, assetsUsedByScenes, scenesSizes);

                BuildInfoProcessorUtils.DiscoverDependenciesAndMissingAtlases(infos, assetsUsedByScenes, detailsCollector);

                if ( detailsCollector != null )
                {
                    BuildInfoProcessorUtils.FinishCollectingDetails(infos, detailsCollector);
                }

                BuildArtifactsInfo artifactsInfo = null;
                try
                {
                    artifactsInfo = BuildArtifactsInfo.Create(target, path);
                }
                catch (Exception ex)
                {
                    Log.Warning("Unable to obtain build artifacts info: {0}", ex);
                }

                BuildInfoProcessorUtils.RefreshScenesInfo(scenesSizes, infos, artifactsInfo, processedScenes, scenesDetails, detailsCollector);

                if (artifactsInfo != null)
                {
                    BuildInfoProcessorUtils.RefreshModulesInfo(infos, artifactsInfo);
                }

                // sort infos based on paths (easier diffs)
                infos.Sort(BuildInfoProcessorUtils.PathComparer);

                if (artifactsInfo != null)
                {
                    BuildInfoProcessorUtils.RefreshOtherArtifacts(infos, artifactsInfo);
                }

                if (detailsCollector != null)
                {
                    EditorUtility.DisplayProgressBar("Better Build Info", "Analyzing build... (compressed sizes)", 1.0f);
                    BuildInfoProcessorUtils.FinishCalculatingCompressedSizes(infos, detailsCollector);
                    BuildInfoProcessorUtils.CalculateScriptReferences(infos, detailsCollector);
                }

                var settings = BuildInfoProcessorUtils.GetPlayerSettings(typeof(PlayerSettings))
                    .Concat(BuildInfoProcessorUtils.GetPlayerSettings(typeof(EditorUserBuildSettings)));

#if UNITY_ANDROID
                settings = settings.Concat(BuildInfoProcessorUtils.GetPlayerSettings(typeof(PlayerSettings.Android)));
#elif UNITY_IOS
                settings = settings.Concat(BuildInfoProcessorUtils.GetPlayerSettings(typeof(PlayerSettings.iOS)));
#endif

                var buildInfo = new BuildInfo()
                {
                    dateUTC = DateTime.UtcNow.Ticks,
                    buildTarget = target.ToString(),
                    projectPath = Environment.CurrentDirectory.Replace('\\', '/'),
                    outputPath = path,
                    unityVersion = Application.unityVersion,

                    buildTime = buildTimer.ElapsedMilliseconds / 1000.0f,
                    overheadTime = toolOverheadTimer.ElapsedMilliseconds / 1000.0f,

                    assets = infos,
                    scenes = processedScenes.Distinct().ToList(),
                    buildSettings = settings.GroupBy(x => x.name).OrderBy(x => x.Key).Select(x => x.FirstOrDefault()).ToList(),
                    environmentVariables = System.Environment.GetEnvironmentVariables().Cast<DictionaryEntry>().Select(x => new BuildSetting()
                    {
                        name = x.Key.ToString(),
                        value = x.Value.ToString()
                    }).OrderBy(x => x.name).ToList(),
                };

                if (artifactsInfo != null)
                {
                    buildInfo.totalSize = artifactsInfo.totalSize.uncompressed;
                    buildInfo.compressedSize = artifactsInfo.totalSize.compressed;
                    buildInfo.streamingAssetsSize = artifactsInfo.streamingAssetsSize;
                    buildInfo.runtimeSize = artifactsInfo.runtimeSize.uncompressed;
                    buildInfo.compressedRuntimeSize = artifactsInfo.runtimeSize.compressed;
                };

                if (config.testMode)
                {
                    buildInfo.dateUTC = 0;
                    buildInfo.projectPath = string.Empty;
                    buildInfo.outputPath = string.Empty;
                    buildInfo.buildTime = 0;
                    buildInfo.overheadTime = 0;
                    buildInfo.totalSize = 0;
                    buildInfo.compressedSize = 0;
                    buildInfo.environmentVariables = new List<BuildSetting>();
                    buildInfo.buildSettings = new List<BuildSetting>();

                    buildInfo.unityVersion = string.Empty;

                    var editorDir = ReliablePath.GetDirectoryName(EditorApplication.applicationPath).TrimEnd('/');
                    foreach (var asset in buildInfo.assets)
                    {
                        if (asset.path.StartsWith(editorDir, StringComparison.OrdinalIgnoreCase))
                        {
                            asset.path = "UnityDir" + asset.path.Substring(editorDir.Length);
                        }
                    }

                    buildInfo.assets.Sort(BuildInfoProcessorUtils.PathComparer);
                }

                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(BuildInfo));

                var outputPath = string.Empty;


                try
                {
                    outputPath = BuildInfoSettings.Instance.GetOutputPath(DateTime.Now, target, path);

                    var dirName = ReliablePath.GetDirectoryName(outputPath);
                    if (!string.IsNullOrEmpty(dirName) && !Directory.Exists(dirName))
                    {
                        Directory.CreateDirectory(dirName);
                    }

                    using (var writer = File.CreateText(outputPath))
                    {
                        serializer.Serialize(writer, buildInfo);
                    }

                    Log.Info("Generated report at: {0}", outputPath);

                    if (BuildInfoSettings.Instance.autoOpenReportAfterBuild)
                    {
                        BuildInfoSettings.ReportToOpen = outputPath;
                    }
                }
                catch (System.Exception ex)
                {
                    var tempPath = ReliablePath.GetTempPath() + "BBI_" + Guid.NewGuid().ToString() + ".bbi";
                    Log.Error("Error saving report at {0}, saving it temporarily at {1}. Copy it manually to a target location. Error details: {2}", outputPath, tempPath, ex);

                    using (var writer = new StreamWriter(tempPath))
                    {
                        serializer.Serialize(writer, buildInfo);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error("Unexpected error: {0}", ex);
            }
            finally
            {
                if (!BuildInfoSettings.Instance.autoOpenReportAfterBuild)
                {
                    BuildInfoSettings.ReportToOpen = null;
                }

                EditorUtility.ClearProgressBar();
            }
        }

        private static void CollectUsedAssets(string sceneName, Dictionary<string, AssetInfo> assets, BuildInfoAssetDetailsCollector collector, out AssetProperty[] sceneDetails)
        {
            List<AssetProperty> details = new List<AssetProperty>();
            Func<string, UnityEngine.Object, AssetInfo> touchEntry = (assetPath, asset) =>
            {
                AssetInfo entry;
                if (!assets.TryGetValue(assetPath, out entry))
                {
                    entry = new AssetInfo()
                    {
                        path = assetPath,
                    };

                    assets.Add(assetPath, entry);
                }

                if (collector != null && entry.details == null)
                {
                    bool isMainAsset = true;
                    if (!AssetDatabase.IsMainAsset(asset) && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(asset)))
                    {
                        isMainAsset = false;
                    }

                    if (isMainAsset)
                    {
                        details.Clear();

                        Log.Debug("Collecting details for asset: {0}", assetPath);
                        collector.CollectForAsset(details, asset, assetPath);
                        entry.details = details.ToArray();
                    }
                    else
                    {
                        Log.Debug("Not a main asset: {0} {1}", asset.name, AssetDatabase.GetAssetPath(asset));
                    }
                }

                if (!string.IsNullOrEmpty(sceneName))
                {
                    int sceneIndex = entry.scenes.BinarySearch(sceneName);
                    if (sceneIndex < 0)
                    {
                        entry.scenes.Insert(~sceneIndex, sceneName);
                    }
                }
                return entry;
            };

            var legacySpriteHandler = UnityVersionAgnostic.IsUsingLegacySpriteAtlases ? BuildInfoProcessorUtils.CreateLegacyAtlasHandler(touchEntry) : null;

            // include inactive ones too
            var sceneRoots = UnityVersionAgnostic.GetSceneRoots();

            sceneDetails = null;
            if (collector != null)
            {
                Log.Debug("Collecting scene details: {0}", sceneName);
                sceneDetails = collector.CollectForCurrentScene(sceneRoots);
            }

            Log.Debug("Processing scene objects for scene: {0}", sceneName);

            IEnumerable<UnityEngine.Object> objects = EditorUtility.CollectDependencies(sceneRoots).Where(x => x);

            foreach (var obj in objects)
            {
                string assetPath;
                var dep = obj;

                if ( !EditorUtility.IsPersistent(dep) )
                {
                    //if (dep is GameObject)
                    //{
                    //    var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(dep);
                    //    Debug.Log("AAA " + prefabPath + " BBB " + dep + " CCC " + PrefabUtility.GetPrefabInstanceStatus(dep));
                    //    if (string.IsNullOrEmpty(prefabPath))
                    //        continue;

                    //    assetPath = prefabPath;
                    //    dep = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    //}
                    //else
                    {
                        continue;
                    }
                }
                else
                {
                    assetPath = AssetDatabase.GetAssetPath(dep);
                }

                if (string.IsNullOrEmpty(assetPath))
                {
                    Log.Debug(dep, "empty path: name: {0}, scene: {1}", dep.name, sceneName);
                    continue;
                }

                touchEntry(assetPath, dep);

                if (legacySpriteHandler != null && dep is UnitySprite)
                {
                    legacySpriteHandler((UnitySprite)dep, assetPath);
                }
            }

            // add lightmaps
            Log.Debug("Processing lightmaps for scene: {0}", sceneName);
            foreach (var data in UnityEngine.LightmapSettings.lightmaps)
            {
                if (data.GetDirectional())
                {
                    touchEntry(AssetDatabase.GetAssetPath(data.GetDirectional()), data.GetDirectional());
                }
                if (data.GetLight())
                {
                    touchEntry(AssetDatabase.GetAssetPath(data.GetLight()), data.GetLight());
                }
            }

            // now check lightmap settings
            var lightmapSettings = BuildInfoProcessorUtils.GetLightmapSettings();

            for (var prop = new SerializedObject(lightmapSettings).GetIterator(); prop.Next(true);)
            {
                if (prop.propertyType == SerializedPropertyType.ObjectReference)
                {
                    var obj = prop.objectReferenceValue;
                    if (obj && EditorUtility.IsPersistent(obj))
                    {
                        string path = AssetDatabase.GetAssetPath(obj);
                        touchEntry(path, obj);
                    }
                }
            }
        }

        internal static List<AssetInfo> GetAssetsAndSizesFromBuildLog(string buildLogPath, Dictionary<string, AssetInfo> assetsUsedByScenes, Dictionary<string, long> sceneSizes)
        {
            List<AssetInfo> assetsInfo = new List<AssetInfo>(assetsUsedByScenes.Values);

            List<string> paths = new List<string>();
            List<long> sizes = new List<long>();
            BuildLogParser.GetLastBuildAssetsSizes(buildLogPath, paths, sizes, sceneSizes);

            for (int i = 0; i < paths.Count; ++i)
            {
                var path = paths[i];

                AssetInfo info;
                if (!assetsUsedByScenes.TryGetValue(path, out info))
                {
                    info = new AssetInfo()
                    {
                        path = path
                    };
                    assetsInfo.Add(info);
                }

                if (info.size != 0)
                {
                    Log.Warning("How come asset {0} already has a size of {1}?", path, info.size);
                }

                info.size = sizes[i];
            }

            return assetsInfo;
        }
    }
}