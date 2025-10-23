#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using IFC.Data;
using IFC.Systems.Profiles;
using UnityEditor;
using UnityEngine;

namespace IFC.EditorTools
{
    internal static class BuildingEditorUtil
    {
        private static string[] _ids;

        public static string[] GetBuildingIds()
        {
            if (_ids != null && _ids.Length > 0) return _ids;
            var list = new List<string>();
            try
            {
                var collection = BuildingDefinitionLoader.LoadFromJson("content/data/buildings.json");
                if (collection != null && collection.buildings != null)
                {
                    for (int i = 0; i < collection.buildings.Count; i++)
                    {
                        var b = collection.buildings[i];
                        if (b != null && !string.IsNullOrEmpty(b.id))
                        {
                            list.Add(b.id);
                        }
                    }
                }
            }
            catch { }

            if (list.Count == 0)
            {
                var soDefs = Resources.LoadAll<BuildingData>(string.Empty);
                for (int i = 0; i < soDefs.Length; i++)
                {
                    string key = string.IsNullOrEmpty(soDefs[i].BuildingName) ? soDefs[i].name : soDefs[i].BuildingName;
                    if (!string.IsNullOrEmpty(key))
                    {
                        list.Add(key);
                    }
                }
            }

            list.Sort(StringComparer.OrdinalIgnoreCase);
            _ids = list.ToArray();
            return _ids;
        }
    }

    [CustomPropertyDrawer(typeof(StartProfileBuildingLevel))]
    public class StartProfileBuildingLevelDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var typeProp = property.FindPropertyRelative("buildingType");
            var levelProp = property.FindPropertyRelative("level");

            float half = position.width * 0.65f;
            var left = new Rect(position.x, position.y, half - 4f, EditorGUIUtility.singleLineHeight);
            var right = new Rect(position.x + half, position.y, position.width - half, EditorGUIUtility.singleLineHeight);

            var ids = BuildingEditorUtil.GetBuildingIds();
            int index = Mathf.Max(0, Array.IndexOf(ids, typeProp.stringValue));
            index = EditorGUI.Popup(left, "Building", index, ids);
            if (index >= 0 && index < ids.Length)
            {
                typeProp.stringValue = ids[index];
            }

            levelProp.intValue = EditorGUI.IntField(right, "Level", Mathf.Max(0, levelProp.intValue));

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + 2f;
        }
    }
}
#endif

