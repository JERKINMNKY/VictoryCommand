using UnityEngine;
using UnityEditor;
using IFC.Data;
using IFC.CitySystem;

public class CityTileEditor : EditorWindow
{
    CityData cityData;
    Vector2 scrollPos;
    string lastValidationMessage = string.Empty;
    int filterIndex = 0;
    string[] filterOptions = new string[] { "Show All", "Core", "Military", "Resource", "Endgame" };
    bool showGrid = true;
    int selectedTileIndex = -1;
    BuildingData selectedAssignBuilding = null;
    int cityGridCols = 0;
    int resGridCols = 0;

    [MenuItem("IFC/City Tile Editor")]
    public static void ShowWindow()
    {
        GetWindow<CityTileEditor>("City Tile Editor");
    }

    void CreateSampleBuildings()
    {
        string dir = "Assets/Resources/TestBuildings";
        if (!AssetDatabase.IsValidFolder(dir))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "TestBuildings");
        }

        for (int i = 0; i < 3; i++)
        {
            var bd = ScriptableObject.CreateInstance<IFC.Data.BuildingData>();
            bd.name = "SampleBuilding_" + i;
            // set private fields via SerializedObject
            var so = new SerializedObject(bd);
            so.FindProperty("_buildingName").stringValue = bd.name;
            so.FindProperty("_level").intValue = 1 + i;
            so.ApplyModifiedProperties();

            string path = System.IO.Path.Combine(dir, bd.name + ".asset");
            AssetDatabase.CreateAsset(bd, path);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    void OnEnable()
    {
        // safe defaults
        if (cityGridCols <= 0) cityGridCols = CityConstants.DefaultCityCols;
        if (resGridCols <= 0) resGridCols = CityConstants.DefaultResCols;
        selectedTileIndex = -1;
    }

    void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("City Tile Editor", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        cityData = (CityData)EditorGUILayout.ObjectField("City Data", cityData, typeof(CityData), false);
        if (EditorGUI.EndChangeCheck())
        {
            lastValidationMessage = string.Empty;
        }

        if (cityData == null)
        {
            EditorGUILayout.HelpBox("Select a CityData asset to edit tiles.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();
        filterIndex = EditorGUILayout.Popup("Filter", filterIndex, filterOptions);
        EditorGUILayout.Space();

        SerializedObject so = new SerializedObject(cityData);
        so.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("City Info", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Name", cityData.cityName);
        EditorGUILayout.LabelField("Level", cityData.cityLevel.ToString());

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Auto-Fill Starter Layout"))
        {
            OnClickAutoFill();
        }
        if (GUILayout.Button("Auto-Assign Grid Positions"))
        {
            OnClickAutoAssign();
        }
        if (GUILayout.Button("Ensure Tiles Size"))
        {
            OnClickEnsureSize();
        }

        if (GUILayout.Button("Ensure Resource Tiles Size"))
        {
            OnClickEnsureSize();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        // Visual grid
            showGrid = EditorGUILayout.Toggle("Show Visual Grid", showGrid);
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            int newCityCols = EditorGUILayout.IntField(new GUIContent("City Grid Columns"), Mathf.Max(1, cityGridCols));
            int newResCols = EditorGUILayout.IntField(new GUIContent("Resource Grid Columns"), Mathf.Max(1, resGridCols));
            if (EditorGUI.EndChangeCheck())
            {
                // clamp to reasonable sizes to avoid huge draws
                newCityCols = Mathf.Clamp(newCityCols, 1, 50);
                newResCols = Mathf.Clamp(newResCols, 1, 50);
                Undo.RecordObject(cityData, "Change Grid Columns");
                cityGridCols = newCityCols;
                resGridCols = newResCols;
                cityData.editorCityGridColumns = cityGridCols;
                cityData.editorResourceGridColumns = resGridCols;
                EditorUtility.SetDirty(cityData);
                AssetDatabase.SaveAssets();
            }
            EditorGUILayout.EndHorizontal();
            if (showGrid)
            {
                DrawCityGrid(so, cityGridCols);
                EditorGUILayout.Space();
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Validate Layout")) OnClickValidate();
            EditorGUILayout.LabelField(lastValidationMessage ?? "", EditorStyles.helpBox);
            EditorGUILayout.EndHorizontal();
            // load persisted selection and grid cols from asset if available
            if (cityData != null)
            {
                if (cityData.editorCityGridColumns > 0) cityGridCols = cityData.editorCityGridColumns;
                if (cityData.editorResourceGridColumns > 0) resGridCols = cityData.editorResourceGridColumns;
                if (cityData.editorLastSelectedTile >= 0 && selectedTileIndex < 0)
                {
                    selectedTileIndex = cityData.editorLastSelectedTile;
                }
            }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        DrawTileList(so, "cityTiles", "City Tiles");
        EditorGUILayout.Space();
        DrawTileList(so, "resourceTiles", "Resource Tiles");

        EditorGUILayout.EndScrollView();

        if (!string.IsNullOrEmpty(lastValidationMessage))
        {
            EditorGUILayout.HelpBox(lastValidationMessage, MessageType.Warning);
        }

        if (so.hasModifiedProperties)
        {
            if (GUILayout.Button("Apply Changes"))
            {
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(cityData);
                AssetDatabase.SaveAssets();
            }
        }

        // Assign building to selected tile (drag & drop friendly via ObjectField)
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tile Assignment", EditorStyles.boldLabel);

        // prefer persisted selection if available
        if (cityData != null)
        {
            selectedTileIndex = cityData.editorLastSelectedTile;
        }

        if (selectedTileIndex >= 0 && cityData != null && selectedTileIndex < cityData.cityTiles.Count)
        {
            EditorGUILayout.LabelField("Selected Tile", selectedTileIndex.ToString());

            // Drag-and-drop compatible assignment field with immediate validation
            EditorGUI.BeginChangeCheck();
            var newAssign = (BuildingData)EditorGUILayout.ObjectField(
                new GUIContent("Assign Building"),
                selectedAssignBuilding,
                typeof(BuildingData),
                false);
            if (EditorGUI.EndChangeCheck())
            {
                // validate if changed and non-null
                if (newAssign != selectedAssignBuilding)
                {
                    if (newAssign != null && HasValidSelection())
                    {
                        int idx = selectedTileIndex;
                        if (cityData != null && idx >= 0 && idx < cityData.cityTiles.Count)
                        {
                            var tile = cityData.cityTiles[idx];
                            if (!CityManager.CanPlaceBuilding(cityData, tile, newAssign, out string reason))
                            {
                                EditorUtility.DisplayDialog("Cannot place building", string.Format("Cannot place {0}: {1}", newAssign.BuildingName, reason), "OK");
                                // clear field to avoid confusion
                                selectedAssignBuilding = null;
                            }
                            else
                            {
                                selectedAssignBuilding = newAssign;
                            }
                        }
                        else
                        {
                            selectedAssignBuilding = newAssign;
                        }
                    }
                    else
                    {
                        selectedAssignBuilding = newAssign;
                    }
                }
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Assign to Selected"))
            {
                AssignToSelected(selectedAssignBuilding);
            }
            if (GUILayout.Button("Clear Selected"))
            {
                ClearSelected();
            }
            EditorGUILayout.EndHorizontal();
            // Diagnostics: show runtime vs serialized assigned building for selected tile
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Diagnostics", EditorStyles.boldLabel);
            var runtimeAssigned = cityData.cityTiles[selectedTileIndex].assignedBuilding;
            EditorGUILayout.LabelField("Runtime assigned:", runtimeAssigned != null ? runtimeAssigned.BuildingName : "None");

            // serialized view
            SerializedObject soLocal = new SerializedObject(cityData);
            soLocal.Update();
            SerializedProperty cityTilesPropLocal = soLocal.FindProperty("cityTiles");
            if (cityTilesPropLocal != null && selectedTileIndex < cityTilesPropLocal.arraySize)
            {
                var elemLocal = cityTilesPropLocal.GetArrayElementAtIndex(selectedTileIndex);
                var assignedPropLocal = elemLocal.FindPropertyRelative("assignedBuilding");
                var serAssigned = assignedPropLocal != null ? (BuildingData)assignedPropLocal.objectReferenceValue : null;
                EditorGUILayout.LabelField("Serialized assigned:", serAssigned != null ? serAssigned.BuildingName : "None");
            }
            else
            {
                EditorGUILayout.LabelField("Serialized assigned:", "(no serialized element)");
            }

            if (GUILayout.Button("Force Apply Serialized Changes"))
            {
                // Re-apply and save
                soLocal.ApplyModifiedProperties();
                EditorUtility.SetDirty(cityData);
                AssetDatabase.SaveAssets();
            }

            // Quick helper: create sample BuildingData assets if none exist
            if (GUILayout.Button("Create Sample Buildings (for testing)"))
            {
                CreateSampleBuildings();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No valid tile selected.", MessageType.Warning);
        }

        so.ApplyModifiedProperties();
    }

    void DrawTileList(SerializedObject so, string propertyName, string label)
    {
        SerializedProperty tilesProp = so.FindProperty(propertyName);
        if (tilesProp == null)
        {
            EditorGUILayout.HelpBox($"Property '{propertyName}' not found on CityData.", MessageType.Error);
            return;
        }

        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        for (int i = 0; i < tilesProp.arraySize; i++)
        {
            var elem = tilesProp.GetArrayElementAtIndex(i);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Tile {i}", EditorStyles.miniBoldLabel);

            var idxProp = elem.FindPropertyRelative("tileIndex");
            var unlockedProp = elem.FindPropertyRelative("isUnlocked");
            var unlockLevelProp = elem.FindPropertyRelative("unlockLevel");
            var assignedProp = elem.FindPropertyRelative("assignedBuilding");
            var gridPosProp = elem.FindPropertyRelative("gridPosition");

            if (idxProp != null) EditorGUILayout.LabelField("Index", idxProp.intValue.ToString());
            if (unlockedProp != null) unlockedProp.boolValue = EditorGUILayout.Toggle("Unlocked", unlockedProp.boolValue);
            if (unlockLevelProp != null) unlockLevelProp.intValue = EditorGUILayout.IntField(new GUIContent("Unlock Level"), Mathf.Max(1, unlockLevelProp.intValue));
            if (assignedProp != null)
            {
                BuildingData current = (BuildingData)assignedProp.objectReferenceValue;

                // Apply filter: skip drawing if it doesn't match
                if (ShouldShowTile(current))
                {
                    assignedProp.objectReferenceValue = (BuildingData)EditorGUILayout.ObjectField("Assigned Building", assignedProp.objectReferenceValue, typeof(BuildingData), false);
                }
                else
                {
                    EditorGUILayout.LabelField("(Filtered)");
                }
            }

            if (gridPosProp != null)
            {
                var gp = gridPosProp.vector2IntValue;
                EditorGUILayout.BeginHorizontal();
                gp.x = EditorGUILayout.IntField("Grid X", gp.x);
                gp.y = EditorGUILayout.IntField("Grid Y", gp.y);
                EditorGUILayout.EndHorizontal();
                gridPosProp.vector2IntValue = gp;
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Validate Placement"))
            {
                BuildingData bd = assignedProp != null ? (BuildingData)assignedProp.objectReferenceValue : null;
                TileData temp = new TileData { tileIndex = idxProp != null ? idxProp.intValue : i, isUnlocked = unlockedProp != null ? unlockedProp.boolValue : false, unlockLevel = unlockLevelProp != null ? unlockLevelProp.intValue : 1, assignedBuilding = bd };
                if (bd == null)
                {
                    lastValidationMessage = "No building assigned to validate.";
                }
                else if (CityManager.CanPlaceBuilding(cityData, temp, bd, out string reason))
                {
                    lastValidationMessage = $"Tile {i}: OK to place {bd.BuildingName}.";
                }
                else
                {
                    lastValidationMessage = $"Tile {i}: {reason}";
                }
            }

            if (GUILayout.Button("Clear"))
            {
                if (assignedProp != null) assignedProp.objectReferenceValue = null;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
    }

    bool ShouldShowTile(BuildingData bd)
    {
        if (filterIndex == 0) return true;
        if (bd == null) return true;
        var t = bd.buildingType;
        switch (filterIndex)
        {
            case 1: return t == BuildingType.Core;
            case 2: return t == BuildingType.Military;
            case 3: return t == BuildingType.Resource;
            case 4: return t == BuildingType.Endgame;
            default: return true;
        }
    }

    void AutoFillStarterLayout(SerializedObject so)
    {
        // Find building assets in project
        string[] guids = AssetDatabase.FindAssets("t:BuildingData");
        BuildingData staff = null, warehouse = null, wall = null;
        var armsList = new System.Collections.Generic.List<BuildingData>();
        var resourceList = new System.Collections.Generic.List<BuildingData>();

        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var bd = AssetDatabase.LoadAssetAtPath<BuildingData>(path);
            if (bd == null) continue;
            if (bd.buildingType == BuildingType.Core)
            {
                if (bd.BuildingName.ToLower().Contains("staff")) staff = bd;
                if (bd.BuildingName.ToLower().Contains("warehouse")) warehouse = bd;
                if (bd.BuildingName.ToLower().Contains("wall")) wall = bd;
            }
            if (bd.buildingType == BuildingType.Military) armsList.Add(bd);
            if (bd.buildingType == BuildingType.Resource) resourceList.Add(bd);
        }

        SerializedProperty cityTilesProp = so.FindProperty("cityTiles");
        if (cityTilesProp != null)
        {
            Undo.RecordObject(cityData, "Auto-Fill Starter Layout");
            // Place core buildings first
            int placed = 0;
            for (int i = 0; i < cityTilesProp.arraySize && placed < 3; i++)
            {
                var elem = cityTilesProp.GetArrayElementAtIndex(i);
                var assigned = elem.FindPropertyRelative("assignedBuilding");
                if (assigned != null && assigned.objectReferenceValue == null)
                {
                    if (placed == 0 && staff != null) assigned.objectReferenceValue = staff;
                    else if (placed == 1 && warehouse != null) assigned.objectReferenceValue = warehouse;
                    else if (placed == 2 && wall != null) assigned.objectReferenceValue = wall;
                    placed++;
                }
            }

            // Place up to 4 arms plants
            int armsPlaced = 0;
            for (int i = 0; i < cityTilesProp.arraySize && armsPlaced < 4; i++)
            {
                var elem = cityTilesProp.GetArrayElementAtIndex(i);
                var assigned = elem.FindPropertyRelative("assignedBuilding");
                if (assigned != null && assigned.objectReferenceValue == null)
                {
                    if (armsList.Count > 0)
                    {
                        assigned.objectReferenceValue = armsList[armsPlaced % armsList.Count];
                        armsPlaced++;
                    }
                }
            }
        }

        // Fill resource tiles round-robin
        SerializedProperty resProp = so.FindProperty("resourceTiles");
        if (resProp != null && resourceList.Count > 0)
        {
            Undo.RecordObject(cityData, "Auto-Fill Resource Tiles");
            int r = 0;
            for (int i = 0; i < resProp.arraySize; i++)
            {
                var elem = resProp.GetArrayElementAtIndex(i);
                var assigned = elem.FindPropertyRelative("assignedBuilding");
                if (assigned != null)
                {
                    assigned.objectReferenceValue = resourceList[r % resourceList.Count];
                    r++;
                }
            }
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(cityData);
        AssetDatabase.SaveAssets();
    }

    // overload for convenience
    void AutoFillStarterLayout()
    {
        if (cityData == null) return;
        SerializedObject so = new SerializedObject(cityData);
        so.Update();
        AutoFillStarterLayout(so);
    }

    void AutoAssignGridPositions(SerializedObject so)
    {
        // City grid: 5 columns x 5 rows
    int cityCols = cityGridCols > 0 ? cityGridCols : 5;

        SerializedProperty cityTilesProp = so.FindProperty("cityTiles");
        if (cityTilesProp != null)
        {
            for (int i = 0; i < cityTilesProp.arraySize; i++)
            {
                var elem = cityTilesProp.GetArrayElementAtIndex(i);
                var gp = elem.FindPropertyRelative("gridPosition");
                if (gp != null)
                {
                    int x = i % cityCols;
                    int y = i / cityCols;
                    gp.vector2IntValue = new UnityEngine.Vector2Int(x, y);
                }
            }
        }

        // Resource grid: 6 columns x 7 rows (6*7 = 42 >= 39)
        int resCols = 6;
        SerializedProperty resProp = so.FindProperty("resourceTiles");
        if (resProp != null)
        {
            Undo.RecordObject(cityData, "Auto-Assign Resource Grid Positions");
            for (int i = 0; i < resProp.arraySize; i++)
            {
                var elem = resProp.GetArrayElementAtIndex(i);
                var gp = elem.FindPropertyRelative("gridPosition");
                if (gp != null)
                {
                    int x = i % resCols;
                    int y = i / resCols;
                    gp.vector2IntValue = new UnityEngine.Vector2Int(x, y);
                }
            }
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(cityData);
        AssetDatabase.SaveAssets();
    }

    // overload convenience
    void AutoAssignGridPositions()
    {
        if (cityData == null) return;
        SerializedObject so = new SerializedObject(cityData);
        so.Update();
        AutoAssignGridPositions(so);
    }

    // Grouped undo helper
    void DoGrouped(string label, UnityEngine.Object target, System.Action body)
    {
        int g = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName(label);
        Undo.RecordObject(target, label);
        try
        {
            body();
        }
        finally
        {
            EditorUtility.SetDirty(target);
            Undo.CollapseUndoOperations(g);
        }
    }

    void OnClickAutoFill()
    {
        DoGrouped("City Editor: Auto-Fill Starter Layout", cityData, () => AutoFillStarterLayout());
    }

    void OnClickAutoAssign()
    {
        DoGrouped("City Editor: Auto-Assign Grid", cityData, () => AutoAssignGridPositions());
    }

    void EnsureCityTilesSize()
    {
        if (cityData == null) return;
        SerializedObject so = new SerializedObject(cityData);
        var cityTilesProp = so.FindProperty("cityTiles");
        if (cityTilesProp != null)
        {
            cityTilesProp.arraySize = CityConstants.MAX_CITY_TILES;
            for (int i = 0; i < cityTilesProp.arraySize; i++)
            {
                var elem = cityTilesProp.GetArrayElementAtIndex(i);
                var idx = elem.FindPropertyRelative("tileIndex");
                if (idx != null) idx.intValue = i;
                var unlocked = elem.FindPropertyRelative("isUnlocked");
                if (unlocked != null && i < cityData.unlockedCityTiles) unlocked.boolValue = true;
            }
            so.ApplyModifiedProperties();
        }
    }

    void EnsureResourceTilesSize()
    {
        if (cityData == null) return;
        SerializedObject so = new SerializedObject(cityData);
        var rProp = so.FindProperty("resourceTiles");
        if (rProp != null)
        {
            rProp.arraySize = CityConstants.MAX_RESOURCE_TILES;
            for (int i = 0; i < rProp.arraySize; i++)
            {
                var elem = rProp.GetArrayElementAtIndex(i);
                var idx = elem.FindPropertyRelative("tileIndex");
                if (idx != null) idx.intValue = i;
            }
            so.ApplyModifiedProperties();
        }
    }

    void OnClickEnsureSize()
    {
        DoGrouped("City Editor: Ensure Tiles Size", cityData, () =>
        {
            EnsureCityTilesSize();
            EnsureResourceTilesSize();
        });
    }

    void DrawCityGrid(SerializedObject so, int cols)
    {
        // safety: avoid drawing extremely large grids
        if (cols <= 0) cols = CityConstants.DefaultCityCols;
        if (cols > 50) cols = 50;
        int rows = Mathf.CeilToInt((float)CityConstants.MAX_CITY_TILES / cols);
        float buttonSize = 60f;
        GUILayout.Label("City Grid", EditorStyles.boldLabel);

        SerializedProperty cityTilesProp = so.FindProperty("cityTiles");
        if (cityTilesProp == null)
        {
            EditorGUILayout.HelpBox("cityTiles not found on CityData.", MessageType.Error);
            return;
        }

        for (int y = 0; y < rows; y++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < cols; x++)
            {
                int idx = y * cols + x;
                if (idx >= cityTilesProp.arraySize)
                {
                    GUILayout.Space(buttonSize);
                    continue;
                }

                var elem = cityTilesProp.GetArrayElementAtIndex(idx);
                var unlockedProp = elem.FindPropertyRelative("isUnlocked");
                var assignedProp = elem.FindPropertyRelative("assignedBuilding");

                bool unlocked = unlockedProp != null && unlockedProp.boolValue;
                var bd = assignedProp != null ? (BuildingData)assignedProp.objectReferenceValue : null;

                Color prev = GUI.backgroundColor;
                GUI.backgroundColor = unlocked ? Color.green : Color.red;

                GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
                btnStyle.fixedWidth = buttonSize;
                btnStyle.fixedHeight = buttonSize;

                string fullName = bd != null ? bd.BuildingName : "Empty";
                var unlockLevelProp = elem.FindPropertyRelative("unlockLevel");
                int unlockLevel = unlockLevelProp != null ? unlockLevelProp.intValue : 1;
                GUIContent gc = new GUIContent(string.Format("{0}\nL{1}\n{2}", idx, unlockLevel, Trunc(fullName, 16)), string.Format("{0}: L{1} • {2}", idx, unlockLevel, fullName));

                Rect rect = GUILayoutUtility.GetRect(buttonSize, buttonSize);
                if (GUI.Button(rect, gc, btnStyle))
                {
                    selectedTileIndex = idx;
                    if (cityData != null) { cityData.editorLastSelectedTile = selectedTileIndex; EditorUtility.SetDirty(cityData); }
                }

                // context menu (right-click)
                Event e = Event.current;
                if (e.type == EventType.ContextClick && rect.Contains(e.mousePosition))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Assign Selected Building"), false, () => AssignToSelected(selectedAssignBuilding));
                    menu.AddItem(new GUIContent("Clear Tile"), false, () => ClearSelected());
                    menu.ShowAsContext();
                    e.Use();
                }

                // invalid placement outline
                // check via CityManager
                var tileDataProp = elem.FindPropertyRelative("isUnlocked");
                TileData td = null;
                try
                {
                    // build a lightweight TileData for validation
                    td = new TileData
                    {
                        tileIndex = idx,
                        isUnlocked = unlocked,
                        unlockLevel = unlockLevel,
                        assignedBuilding = bd
                    };
                }
                catch { td = null; }

                if (td != null && bd != null && !CityManager.CanPlaceBuilding(cityData, td, bd, out string _))
                {
                    Handles.BeginGUI();
                    var outlineColor = Color.red;
                    Handles.DrawSolidRectangleWithOutline(rect, Color.clear, outlineColor);
                    Handles.EndGUI();
                }

                GUI.backgroundColor = prev;
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    string Trunc(string s, int max = 16)
    {
        if (string.IsNullOrEmpty(s) || s.Length <= max) return s;
        return s.Substring(0, max - 1) + "…";
    }

    void AssignToSelected(BuildingData b)
    {
        if (!HasValidSelection()) return;

        // if assigning a real building, validate first
        if (b != null)
        {
            int idx = selectedTileIndex;
            // Use the existing tile data for validation
            if (cityData != null && idx >= 0 && idx < cityData.cityTiles.Count)
            {
                var tile = cityData.cityTiles[idx];
                if (!CityManager.CanPlaceBuilding(cityData, tile, b, out string reason))
                {
                    EditorUtility.DisplayDialog("Cannot place building", string.Format("Cannot place {0}: {1}", b.BuildingName, reason), "OK");
                    return;
                }
            }
        }

        DoGrouped("City Editor: Assign Tile", cityData, () =>
        {
            int idx = selectedTileIndex;
            SerializedObject so = new SerializedObject(cityData);
            var cityTilesProp = so.FindProperty("cityTiles");
            var elem = cityTilesProp.GetArrayElementAtIndex(idx);
            var assigned = elem.FindPropertyRelative("assignedBuilding");
            if (assigned != null)
            {
                assigned.objectReferenceValue = b;
                so.ApplyModifiedProperties();
                cityData.editorLastSelectedTile = idx;
            }
        });
    }

    void ClearSelected()
    {
        if (!HasValidSelection()) return;
        DoGrouped("City Editor: Clear Tile", cityData, () =>
        {
            int idx = selectedTileIndex;
            SerializedObject so = new SerializedObject(cityData);
            var cityTilesProp = so.FindProperty("cityTiles");
            var elem = cityTilesProp.GetArrayElementAtIndex(idx);
            var assigned = elem.FindPropertyRelative("assignedBuilding");
            if (assigned != null)
            {
                assigned.objectReferenceValue = null;
                so.ApplyModifiedProperties();
                cityData.editorLastSelectedTile = idx;
            }
        });
    }

    bool HasValidSelection()
    {
        if (cityData == null) return false;
        int idx = selectedTileIndex;
        if (idx < 0) return false;
        var so = new SerializedObject(cityData);
        var cityTilesProp = so.FindProperty("cityTiles");
        if (cityTilesProp == null) return false;
        if (idx >= cityTilesProp.arraySize) return false;
        return true;
    }

    // Validation
    bool ValidateLayout(out int badIndex)
    {
        badIndex = -1;
        if (cityData == null) return true;
        for (int i = 0; i < cityData.cityTiles.Count; i++)
        {
            var t = cityData.cityTiles[i];
            var b = t.assignedBuilding;
            if (b == null) continue;
            // Use CityManager.CanPlaceBuilding with a TileData wrapper
            if (!CityManager.CanPlaceBuilding(cityData, t, b, out string reason))
            {
                badIndex = i;
                return false;
            }
        }
        return true;
    }

    void OnClickValidate()
    {
        if (ValidateLayout(out int bad))
        {
            lastValidationMessage = "Layout OK ✓";
        }
        else
        {
            selectedTileIndex = bad;
            if (cityData != null) cityData.editorLastSelectedTile = bad;
            lastValidationMessage = $"Invalid at tile {bad} — check placement rules.";
            Repaint();
        }
    }
}
