/*
 Copyright (c) 2019 Wrnch Inc.
 All rights reserved
*/

using UnityEditor;
using UnityEngine;
using wrnchAI.Core;

namespace wrnchAI.Editor
{
    [CustomEditor(typeof(PoseManager))]
    class PoseManagerEditor : UnityEditor.Editor
    {
        private int m_device;

        private SerializedProperty m_tposePrefab;
        private SerializedProperty m_displayAvatars;

        private SerializedProperty m_poseworkerconfig;
        private SerializedProperty m_videoControllerConfig;
        private SerializedProperty m_videoVisualizerConfigPrefab;

        private void OnEnable()
        {
            m_videoControllerConfig = serializedObject.FindProperty("m_videoControllerConfig");
            m_tposePrefab = serializedObject.FindProperty("m_defaultCharacter");
            m_displayAvatars = serializedObject.FindProperty("m_displayAvatars");
            m_poseworkerconfig = serializedObject.FindProperty("m_poseWorkerConfig");
            m_videoVisualizerConfigPrefab = serializedObject.FindProperty("m_visualizerConfig");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("TPose Prefab");
            m_tposePrefab.objectReferenceValue = (GameObject)EditorGUILayout.ObjectField(m_tposePrefab.objectReferenceValue, typeof(GameObject), false);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Display Avatars:");
            m_displayAvatars.boolValue = EditorGUILayout.Toggle(m_displayAvatars.boolValue);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(m_poseworkerconfig);
            EditorGUILayout.PropertyField(m_videoControllerConfig);
            EditorGUILayout.PropertyField(m_videoVisualizerConfigPrefab);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
