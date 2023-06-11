using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Selector))]
public class SelectorDrawer : PropertyDrawer {
    private const float VerticalSpacing = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);
        position.height = EditorGUIUtility.singleLineHeight;

        SerializedProperty enumProperty = property.FindPropertyRelative("type");
        EditorGUI.PropertyField(position, enumProperty, GUIContent.none);

        SelectorType selectorOption = (SelectorType)enumProperty.enumValueIndex;
        switch (selectorOption) {
            case SelectorType.Circle:
                DrawCircleFields(position, property);
                break;

            case SelectorType.Line:
                DrawLineFields(position, property);
                break;

            default:
                break;
        }

        EditorGUI.EndProperty();
    }

    private void DrawCircleFields(Rect position, SerializedProperty property) {
        position.y += EditorGUIUtility.singleLineHeight + VerticalSpacing;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("range"));
    }

    private void DrawLineFields(Rect position, SerializedProperty property) {
        position.y += EditorGUIUtility.singleLineHeight + VerticalSpacing;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("range"));
        position.y += EditorGUIUtility.singleLineHeight + VerticalSpacing;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("rotIndex"));
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        float totalHeight = EditorGUIUtility.singleLineHeight;

        SerializedProperty enumProperty = property.FindPropertyRelative("type");

        SelectorType selectorOption = (SelectorType)enumProperty.enumValueIndex;
        switch (selectorOption) {
            case SelectorType.Circle:
                totalHeight += (EditorGUIUtility.singleLineHeight + VerticalSpacing) * 1;
                break;

            case SelectorType.Line:
                totalHeight += (EditorGUIUtility.singleLineHeight + VerticalSpacing) * 2;
                break;

            default:
                break;
        }

        return totalHeight;
    }
}