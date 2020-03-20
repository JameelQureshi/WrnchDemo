/*
 Copyright (c) 2019 Wrnch Inc.
 All rights reserved
*/

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using wrnchAI.Core;

#if UNITY_EDITOR

public class QuaternionOverrideDisplay
{
    public bool activateOverride = false;

    private GameObject m_objectToDisplay;
    private Quaternion m_overridenRotation = new Quaternion();
    private float m_quatNumbersSizeBox = 50.0f;

    public QuaternionOverrideDisplay(GameObject referencedGO)
    {
        m_objectToDisplay = referencedGO;
    }

    public void UpdateValues()
    {
        if (activateOverride)
        {
            m_objectToDisplay.transform.rotation = m_overridenRotation;
        }
        else
        {
            m_overridenRotation = m_objectToDisplay.transform.rotation;
        }
    }
    public void UpdateVisual()
    {
        CreateQuatVisual(ref m_overridenRotation, activateOverride);
    }

    private void CreateQuatVisual(ref Quaternion quat, bool OverrideValues)
    {

        GUILayout.BeginHorizontal();
        GUILayout.TextField(m_objectToDisplay.name, GUILayout.Width(150.0f));
        activateOverride = EditorGUILayout.BeginToggleGroup("Override", activateOverride);

        CreateQuatField(ref quat.x, "X", OverrideValues);
        CreateQuatField(ref quat.y, "Y", OverrideValues);
        CreateQuatField(ref quat.z, "Z", OverrideValues);
        CreateQuatField(ref quat.w, "W", OverrideValues);
        GUILayout.EndHorizontal();
        EditorGUILayout.EndToggleGroup();
    }

    private void CreateQuatField(ref float x, string Name, bool overrideValues)
    {
        GUILayout.BeginHorizontal();
        if (overrideValues)
        {
            x = GUILayout.HorizontalSlider(x, -1.0f, 1.0f, GUILayout.Width(150.0f));
        }
        else
        {
            GUILayout.HorizontalSlider(x, -1.0f, 1.0f, GUILayout.Width(150.0f));
        }
        GUILayout.TextField(x.ToString(), GUILayout.Width(m_quatNumbersSizeBox));
        GUILayout.Label(Name, GUILayout.Width(m_quatNumbersSizeBox));
        GUILayout.EndHorizontal();
    }

}

public class JointsTester : EditorWindow
{
    private bool autoselect;
    private AnimationController ControllerToModify;
    List<QuaternionOverrideDisplay> QuatDisplays = new List<QuaternionOverrideDisplay>();
    Vector2 scrollViewPosition = new Vector2(0, 0);

    private void Update()
    {
        Repaint();
    }

    private void OnGUI()
    {
        GUILayout.Label("Settings", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        ControllerToModify = (AnimationController)EditorGUILayout.ObjectField("Avatar", ControllerToModify, typeof(AnimationController), true);
        GUILayout.EndHorizontal();
        if (ControllerToModify != null)
        {
            scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition);
            if (QuatDisplays.Count == 0)
            {
                foreach (GameObject go in ControllerToModify.JointsToRig)
                {
                    QuaternionOverrideDisplay qod = new QuaternionOverrideDisplay(go);
                    ControllerToModify.JointsUpdateFinished += qod.UpdateValues;
                    QuatDisplays.Add(qod);
                }
            }
            else
            {
                foreach (QuaternionOverrideDisplay qod in QuatDisplays)
                {
                    qod.UpdateVisual();
                    GUILayout.Space(50.0f);
                }
            }
            GUILayout.EndScrollView();
        }
        else
        {
            ClearQODs();
            QuatDisplays.Clear();
        }
    }

    private void ClearQODs()
    {
        foreach (QuaternionOverrideDisplay qod in QuatDisplays)
        {
            ControllerToModify.JointsUpdateFinished -= qod.UpdateValues;
        }
    }

    private void OnDestroy()
    {
        ClearQODs();
    }

    [MenuItem("Window/QuatTester")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(JointsTester));
    }
}

#endif