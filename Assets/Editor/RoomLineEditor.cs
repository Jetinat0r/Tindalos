using UnityEngine;
using UnityEditor;

//[CustomPropertyDrawer(typeof(RoomLine))]
public class RoomLineEditor : PropertyDrawer
{
    //------Vars--------
    //SerializedProperty CurLineType;         //Enum

    //SerializedProperty lineStart;           //Vector2
    //SerializedProperty lineEnd;             //RoomLine

    //SerializedProperty bezierStartHandle;   //Vector2
    //SerializedProperty bezierEndHandle;     //Vector2

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        //GUILayout.Label($"Cur Line Type = {property.FindPropertyRelative("CurLineType")}");

        var amountRect = new Rect(position.x, position.y, 30, position.height);
        var unitRect = new Rect(position.x + 35, position.y, 50, position.height);
        var nameRect = new Rect(position.x + 90, position.y, position.width - 90, position.height);

        // Draw fields - pass GUIContent.none to each so they are drawn without labels
        EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("CurLineType"), GUIContent.none);
        EditorGUI.PropertyField(unitRect, property.FindPropertyRelative("startLine"), GUIContent.none);
        EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("startBezierHandle"), GUIContent.none);

        /*
        //Edit self
        property.FindPropertyRelative("lineStart").vector2Value = EditorGUILayout.Vector2Field("Start Point",
            property.FindPropertyRelative("lineStart").vector2Value);
        //Edit next
        RoomLine endPoint = property.FindPropertyRelative("lineEnd").managedReferenceValue as RoomLine;
        if(endPoint != null)
        {
            //Temporary
            endPoint.lineStart = EditorGUILayout.Vector2Field("End Point", endPoint.lineStart);
        }

        //Wish I knew how to do this better
        if (property.FindPropertyRelative("CurLineType").enumValueIndex == (int)LineType.Bezier)
        {
            //Edit handles
            property.FindPropertyRelative("bezierStartHandle").vector2Value = EditorGUILayout.Vector2Field("Bezier Start Handle",
                property.FindPropertyRelative("bezierStartHandle").vector2Value);
            property.FindPropertyRelative("bezierEndHandle").vector2Value = EditorGUILayout.Vector2Field("Bezier End Handle",
                property.FindPropertyRelative("bezierEndHandle").vector2Value);
        }
        */

        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }

    //This will need to be adjusted based on what you are displaying
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return (20 - EditorGUIUtility.singleLineHeight) + (EditorGUIUtility.singleLineHeight * 8);
    }
}
