#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class UniqueIDValidator
{
    static UniqueIDValidator()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.ExitingEditMode)
                CheckUniqueIDsImmediate();
        };
    }
    public static void CheckUniqueIDsImmediate()
    {
        Dictionary<System.Type, HashSet<int>> classIDs = new();
        bool hasDuplicate = false;

        // 1. Hem MonoBehaviour hem ScriptableObject kapsayan tüm varlýklarý al
        var allObjects = Resources.FindObjectsOfTypeAll<UnityEngine.Object>()
            .Where(obj => obj is MonoBehaviour || obj is ScriptableObject)
            .ToArray();

        foreach (var obj in allObjects)
        {
            var type = obj.GetType();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
            {
                if (field.GetCustomAttribute(typeof(UniqueAttribute)) is not null)
                {
                    if (field.FieldType != typeof(int))
                    {
                        Debug.LogError($"[UniqueID] attribute only supports int fields! Problem in {obj.name} ({type.Name})");
                        continue;
                    }

                    int value = (int)field.GetValue(obj);

                    if (!classIDs.ContainsKey(type))
                        classIDs[type] = new HashSet<int>();

                    if (classIDs[type].Contains(value))
                    {
                        Debug.LogError($"Duplicate UniqueID {value} found in class '{type.Name}' on object '{obj.name}'");
                        hasDuplicate = true;
                    }
                    else
                    {
                        classIDs[type].Add(value);
                    }
                }
            }
        }

        if (!hasDuplicate)
            Debug.Log("UniqueID validation passed: No duplicates found.");
    }

}
#endif
