using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Selector))]
public class SelectorDrawer : PropertyDrawer {
    private const float VerticalSpacing = 2f;
    private float totalHeight = EditorGUIUtility.singleLineHeight;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        totalHeight = EditorGUIUtility.singleLineHeight;
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
        totalHeight += EditorGUIUtility.singleLineHeight + VerticalSpacing;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("includeCentreTile"));
        position.y += EditorGUIUtility.singleLineHeight + VerticalSpacing;
        totalHeight += EditorGUIUtility.singleLineHeight + VerticalSpacing;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("range"));
    }

    private void DrawLineFields(Rect position, SerializedProperty property) {
        position.y += EditorGUIUtility.singleLineHeight + VerticalSpacing;
        totalHeight += EditorGUIUtility.singleLineHeight + VerticalSpacing;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("includeCentreTile"));
        position.y += EditorGUIUtility.singleLineHeight + VerticalSpacing;
        totalHeight += EditorGUIUtility.singleLineHeight + VerticalSpacing;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("range"));
        position.y += EditorGUIUtility.singleLineHeight + VerticalSpacing;
        totalHeight += EditorGUIUtility.singleLineHeight + VerticalSpacing;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("AllDirections"));
        if (!property.FindPropertyRelative("AllDirections").boolValue) {
            position.y += EditorGUIUtility.singleLineHeight + VerticalSpacing;
            totalHeight += EditorGUIUtility.singleLineHeight + VerticalSpacing;
            EditorGUI.IntSlider(position, property.FindPropertyRelative("rotIndex"), 0, 5);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return totalHeight;
    }
}