using Attributes;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DrawIfFalseAttribute))]
public class DrawIfFalsePropertyDrawer : PropertyDrawer
{

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        DrawIfFalseAttribute a = (DrawIfFalseAttribute)attribute;
        bool show = ActivateProperty(a, property);
        if (show == true)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        DrawIfFalseAttribute a = (DrawIfFalseAttribute)attribute;
        bool show = ActivateProperty(a, property);
        if (show == true)
        {
            return base.GetPropertyHeight(property, label) + 30;
        }
        return 0f;
    }

    private bool ActivateProperty(DrawIfFalseAttribute attribute, SerializedProperty property)
    {
        bool activate = true;
        string conditionPath = property.propertyPath.Replace(property.name, attribute.VariableName);
        SerializedProperty sourceProperty = property.serializedObject.FindProperty(conditionPath);
        if (sourceProperty != null)
        {
            activate = !sourceProperty.boolValue;
        }
        return activate;
    }
}