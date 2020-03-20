using UnityEditor;
using UnityEngine;
using wrnchAI.Config;

namespace wrnchAI.Editor
{
    [CustomPropertyDrawer(typeof(VideoVisualizerConfig))]
    class VideoVisualizerConfigDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 6 * EditorGUIUtility.singleLineHeight;
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

            DrawProperty("Visualizer", ref position, property.FindPropertyRelative("m_visualizer"));
            DrawProperty("Skeletons", ref position, property.FindPropertyRelative("m_displaySkeletons"));
            DrawProperty("Mirror X", ref position, property.FindPropertyRelative("m_mirrorX"));
            DrawProperty("Mirror Y", ref position, property.FindPropertyRelative("m_mirrorY"));
            DrawProperty("Rotation", ref position, property.FindPropertyRelative("m_rotationMultipleOf90"));

            EditorGUI.EndProperty();
        }
    }
}