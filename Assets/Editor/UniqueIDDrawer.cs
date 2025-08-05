#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(UniqueAttribute))]
public class UniqueIDDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginChangeCheck();

        int newValue = EditorGUI.IntField(position, label, property.intValue);

        if (EditorGUI.EndChangeCheck())
        {
            property.intValue = newValue;
            property.serializedObject.ApplyModifiedProperties();

            EditorApplication.delayCall += () =>
            {
                UniqueIDValidator.CheckUniqueIDsImmediate();
            };
        }
    }
}
#endif