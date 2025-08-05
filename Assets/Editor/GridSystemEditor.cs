using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridSystem))]
public class GridSystemEditor : Editor
{
    private GridSystem gridSystem;
    private Vector2Int? currentHoveredCell = null;

    private void OnEnable()
    {
        gridSystem = (GridSystem)target;
        SceneView.duringSceneGui += DuringSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= DuringSceneGUI;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);
        GUILayout.Label("create grid details", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("create grid details"))
        {
            //butona basınca çalışan yer.
            gridSystem.PlaceMarkedCubes();
        }
    }

    // Sahne içinde tıklama ve hover işlemleri
    private void DuringSceneGUI(SceneView sceneView)
    {
        if (gridSystem == null || gridSystem.targetObject == null) return;

        Event e = Event.current;

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector2Int index = gridSystem.GetGridIndexFromWorld(hit.point);
            if (!gridSystem.IsValidCell(index)) return;

            currentHoveredCell = index;

            if (e.type == EventType.MouseDown && e.button == 0) // Sol tık
            {
                if (e.shift)
                {
                    AddCell(index, gridSystem.soapCells); // Shift -> Soap
                }
                else if (e.control || e.command)
                {
                    AddCell(index, gridSystem.obstacleCells); // Ctrl/Cmd -> Obstacle
                }
                else
                {
                    AddCell(index, gridSystem.boxCells); // Normal -> Box
                }

                e.Use();
            }
            else if (e.type == EventType.MouseDown && e.button == 1) // Sağ tık
            {
                RemoveFromAllLists(index);
                RemovePrefabAt(index);
                e.Use();
            }
        }
        else
        {
            currentHoveredCell = null;
        }

        DrawHighlight();
    }

    private void AddCell(Vector2Int index, System.Collections.Generic.List<Vector2Int> list)
    {
        Undo.RecordObject(gridSystem, "Add Grid Cell");

        if (!list.Contains(index))
        {
            list.Add(index);

            // Diğer listelerden çıkar
            if (list != gridSystem.boxCells)
                gridSystem.boxCells.Remove(index);
            if (list != gridSystem.soapCells)
                gridSystem.soapCells.Remove(index);
            if (list != gridSystem.obstacleCells)
                gridSystem.obstacleCells.Remove(index);

            EditorUtility.SetDirty(gridSystem);
        }
    }

    private void DrawHighlight()
    {
        if (currentHoveredCell.HasValue)
        {
            Vector3 center = gridSystem.GetCellCenter(currentHoveredCell.Value);
            Vector3 size = new Vector3(gridSystem.cellSize, 0.02f, gridSystem.cellSize);
            Handles.color = Color.yellow;
            Handles.DrawSolidRectangleWithOutline(
                new Vector3[] {
                    center + new Vector3(-size.x, 0, -size.z) * 0.5f,
                    center + new Vector3(-size.x, 0,  size.z) * 0.5f,
                    center + new Vector3( size.x, 0,  size.z) * 0.5f,
                    center + new Vector3( size.x, 0, -size.z) * 0.5f
                },
                new Color(1f, 1f, 0f, 0.1f),
                Color.yellow
            );
        }
    }

    private void RemoveFromAllLists(Vector2Int index)
    {
        Undo.RecordObject(gridSystem, "Remove Grid Cell");
        gridSystem.boxCells.Remove(index);
        gridSystem.soapCells.Remove(index);
        gridSystem.obstacleCells.Remove(index);
        EditorUtility.SetDirty(gridSystem);
    }

    private void RemovePrefabAt(Vector2Int index)
    {
        string[] names = {
            gridSystem.boxPrefab?.name,
            gridSystem.soapPrefab?.name,
            gridSystem.obstaclePrefab?.name
        };

        foreach (var baseName in names)
        {
            if (string.IsNullOrEmpty(baseName)) continue;
            string fullName = baseName + "_" + index;
            GameObject found = GameObject.Find(fullName);
            if (found != null)
            {
                Undo.DestroyObjectImmediate(found);
                break;
            }
        }
    }
}