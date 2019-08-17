using UnityEditor;
using UnityEngine;

using UnityInspector.PropertyAttributes;

[CustomPropertyDrawer(typeof(DrawIfTrueAttribute))]
public class DrawIfTruePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        DrawIfTrueAttribute a = (DrawIfTrueAttribute)attribute;
        bool show = ActivateProperty(a, property);
        if (show == true)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        DrawIfTrueAttribute a = (DrawIfTrueAttribute)attribute;
        bool show = ActivateProperty(a, property);
        if (show == true)
        {
            return base.GetPropertyHeight(property, label);
        }
        return 0f;
    }

    private bool ActivateProperty(DrawIfTrueAttribute attribute, SerializedProperty property)
    {
        bool activate = true;
        string conditionPath = property.propertyPath.Replace(property.name, attribute.VariableName);
        SerializedProperty sourceProperty = property.serializedObject.FindProperty(conditionPath);
        if (sourceProperty != null)
        {
            activate = sourceProperty.boolValue;
        }
        return activate;
    }

}
