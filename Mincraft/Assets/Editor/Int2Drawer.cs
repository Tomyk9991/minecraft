using System;
using UnityEditor;
using UnityEngine;

using Core.Math;

[CustomPropertyDrawer(typeof(Int2))]
public class Int2Drawer : PropertyDrawer
{
    private SerializedProperty X, Y;
    private string name;
    private bool cache = false;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginChangeCheck();
        if (!cache)
        {
            //get the name before it's gone
            name = property.displayName;
            
            //get the X, Y, Z values
            property.Next(true);
            X = property.Copy();
            property.Next(true);
            Y = property.Copy();

            cache = true;
        }

        Rect contentPosition = EditorGUI.PrefixLabel(position, new GUIContent(name));
        
        //Check if there is enough space to put the name on the same line (to save space)
        if (position.height > 18f)
        {
            position.height = 18f;
            EditorGUI.indentLevel += 1;
            contentPosition = EditorGUI.IndentedRect(position);
            contentPosition.y += 18f;
        }
        
        float half = contentPosition.width / 2;
        GUI.skin.label.padding = new RectOffset(3, 3, 6, 6);
        
        //show the X and Y from the point
        EditorGUIUtility.labelWidth = 14f;
        contentPosition.width *= 0.5f;
        EditorGUI.indentLevel = 0;
        
        // Begin/end property & change check make each field
        // behave correctly when multi-object editing.
        EditorGUI.BeginProperty(contentPosition, label, X);
        {
            EditorGUI.BeginChangeCheck();
            int newVal = EditorGUI.IntField(contentPosition, new GUIContent("X"), X.intValue);
            if (EditorGUI.EndChangeCheck())
                X.intValue = newVal;
        }
        EditorGUI.EndProperty();

        contentPosition.x += half;

        EditorGUI.BeginProperty(contentPosition, label, Y);
        {
            EditorGUI.BeginChangeCheck();
            int newVal = EditorGUI.IntField(contentPosition, new GUIContent("Y"), Y.intValue);
            if (EditorGUI.EndChangeCheck())
                Y.intValue = newVal;
        }
        EditorGUI.EndProperty();

        
        if (EditorGUI.EndChangeCheck())
        {
            int factor = PlayerPrefs.GetInt("chunkSize");
            X.intValue = ClosestValueToFactor(X.intValue, factor);
            Y.intValue = ClosestValueToFactor(Y.intValue, factor);
        }
    }

    private int ClosestValueToFactor(int value, int factor)
        => (int)Math.Round((value / (double)factor), MidpointRounding.AwayFromZero) * factor;
}
