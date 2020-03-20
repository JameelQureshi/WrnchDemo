using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using wrnchAI.Config;
using wrnchAI.Core;

namespace wrnchAI.Editor
{
    [CustomPropertyDrawer(typeof(VideoControllerConfig))]
    class VideoControllerConfigDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 8 * EditorGUIUtility.singleLineHeight;
        }

        private void DrawProperty(string label, ref Rect position, SerializedProperty prop)
        {
            var newPosition = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent(label));
            EditorGUI.PropertyField(new Rect(newPosition.x, newPosition.y, newPosition.width, EditorGUIUtility.singleLineHeight), prop, GUIContent.none);
            position.y += EditorGUIUtility.singleLineHeight;
        }

        private void DeviceNameFoldout(string label, ref Rect position, SerializedProperty prop)
        {
            var newPosition = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent(label));
            List<string> devices = WebCamTexture.devices.Select(d => d.name).ToList();
            devices.Insert(0, "Auto");
            var deviceIdx = Math.Max(0, devices.IndexOf(prop.stringValue));
            deviceIdx = EditorGUI.Popup(new Rect(newPosition.x, newPosition.y, newPosition.width, EditorGUIUtility.singleLineHeight), deviceIdx, devices.ToArray());
            prop.stringValue = devices[deviceIdx];
            position.y += EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            DrawProperty("VideoMode", ref position, property.FindPropertyRelative("m_videoMode"));
            DeviceNameFoldout("Device", ref position, property.FindPropertyRelative("m_deviceName"));
            DrawProperty("Width", ref position, property.FindPropertyRelative("m_desiredWidth"));
            DrawProperty("Height", ref position, property.FindPropertyRelative("m_desiredHeight"));
            DrawProperty("MirrorX", ref position, property.FindPropertyRelative("m_mirrorX"));
            DrawProperty("MirrorY", ref position, property.FindPropertyRelative("m_mirrorY"));
            DrawProperty("Rotation", ref position, property.FindPropertyRelative("m_rotationMultipleOf90"));
            DrawProperty("VideoURL", ref position, property.FindPropertyRelative("m_videoURL"));
            EditorGUI.EndProperty();
        }
    }
}
