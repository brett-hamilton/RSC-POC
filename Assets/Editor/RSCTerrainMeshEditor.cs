using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RSCTerrainMesh))]
public class RSCTerrainMeshEditor : Editor
{
    TileType paintType = TileType.Grass;
    bool painting = false;

    void OnEnable()
    {
        RSCTerrainMesh terrain = (RSCTerrainMesh)target;
        terrain.EnsureInitialized();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RSCTerrainMesh terrain = (RSCTerrainMesh)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tile Painter", EditorStyles.boldLabel);

        paintType = (TileType)EditorGUILayout.EnumPopup("Paint Type", paintType);

        painting = GUILayout.Toggle(painting, painting ? "Painting (click ON) - click in Scene view" : "Click to enable painting", "Button");

        if (GUILayout.Button("Regenerate Terrain"))
        {
            terrain.EnsureInitialized();
            terrain.GenerateMesh();
            SceneView.RepaintAll();
        }

        EditorGUILayout.HelpBox("With painting enabled, left-click tiles in the Scene view to assign the selected type. Hold and drag to paint multiple tiles.", MessageType.Info);
    }

    void OnSceneGUI()
    {
        if (Event.current.type == EventType.Repaint) return; // skip spam from repaint
            Debug.Log("OnSceneGUI event: " + Event.current.type);

        Debug.Log("OnSceneGUI called, painting=" + painting);
        if (!painting) return;

        RSCTerrainMesh terrain = (RSCTerrainMesh)target;
        Event e = Event.current;

        // Prevent Unity's default click-to-select from eating the click
        int controlId = GUIUtility.GetControlID(FocusType.Passive);
        HandleUtility.AddDefaultControl(controlId);

        if (e.type == EventType.MouseDown || (e.type == EventType.MouseDrag && e.button == 0))
        {
            Debug.Log("Click detected in scene view");
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000f))
            {
                Debug.Log("Hit: " + hit.point);
                // convert world hit point to local tile coords
                Vector3 local = terrain.transform.InverseTransformPoint(hit.point);
                int tileX = Mathf.FloorToInt(local.x / terrain.tileSize);
                int tileZ = Mathf.FloorToInt(local.z / terrain.tileSize);

                terrain.SetTileType(tileX, tileZ, paintType);
                e.Use(); // consume the event so it doesn't also select/deselect
            }
        }
    }
}