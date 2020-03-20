using UnityEngine;
using UnityEditor;
using wrnchAI.Config;

namespace wrnchAI.Editor
{
    [CustomPropertyDrawer(typeof(PoseWorkerConfig))]
    class PoseWorkerConfigDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 10;
        }

        private void DrawProperty(string label, ref Rect position, SerializedProperty prop)
        {
            var newPosition = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent(label));
            EditorGUI.PropertyField(new Rect(newPosition.x, newPosition.y, newPosition.width, EditorGUIUtility.singleLineHeight), prop, GUIContent.none);
            position.y += EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            DrawProperty("Multi 3D", ref position, property.FindPropertyRelative("m_multi3D"));
            DrawProperty("Greenscreen", ref position, property.FindPropertyRelative("m_estimateMask"));
            DrawProperty("Joints Smoothing", ref position, property.FindPropertyRelative("m_jointSmoothing"));
            DrawProperty("Head Smoothing", ref position, property.FindPropertyRelative("m_headSmoothing"));
            DrawProperty("EstimateHead", ref position, property.FindPropertyRelative("m_estimateHead"));
            DrawProperty("Tracker Kind", ref position, property.FindPropertyRelative("m_trackerKind"));
            DrawProperty("Rotation", ref position, property.FindPropertyRelative("m_rotationMultipleOf90"));
            DrawProperty("Net resolution", ref position, property.FindPropertyRelative("m_netRes"));
            DrawProperty("SerializedModelPath", ref position, property.FindPropertyRelative("m_serializedModelPath"));
            DrawProperty("License string", ref position, property.FindPropertyRelative("m_licenseString"));
            EditorGUI.EndProperty();
        }
    }
}
