/*
 Copyright (c) 2019 Wrnch Inc.
 All rights reserved
*/

using UnityEditor;
using UnityEngine;
using wrnchAI.Core;
using wrnchAI.wrAPI;

namespace wrnchAI.Editor
{
    [CustomEditor(typeof(AnimationController))]
    public class AnimationControllerEditor : UnityEditor.Editor
    {
        private bool m_jointsFoldout = true;

        private SerializedProperty m_idProp;

        private void OnEnable()
        {
            m_idProp = serializedObject.FindProperty("m_id");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var controller = (AnimationController)target;

            EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("ID");
                m_idProp.intValue = EditorGUILayout.IntField(m_idProp.intValue);
            EditorGUILayout.EndHorizontal();
            
            if(m_jointsFoldout = EditorGUILayout.Foldout(m_jointsFoldout, "joints map"))
            {
                var jointsList = wrExtended.ToStringArray();
                for (int i = 0; i < jointsList.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel(jointsList[i]);
                           controller.JointsToRig[i] = (GameObject)EditorGUILayout.ObjectField(controller.JointsToRig[i], typeof(GameObject), true);
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorUtility.SetDirty(this);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
