#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WaypointGraph))]
public class WaypointGraphEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        WaypointGraph graph = (WaypointGraph)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("=== Waypoint Tools ===", EditorStyles.boldLabel);

        // Nút thêm waypoint mới
        if (GUILayout.Button("➕ Add New Waypoint"))
        {
            GameObject newWP = new GameObject("Waypoint_" + graph.transform.childCount);
            newWP.transform.parent = graph.transform;
            newWP.transform.position = Vector3.zero;

            // Thêm component Waypoint
            Waypoint waypoint = newWP.AddComponent<Waypoint>();
            if (graph.waypoints == null)
                graph.waypoints = new System.Collections.Generic.List<Waypoint>();
            graph.waypoints.Add(waypoint);

            EditorUtility.SetDirty(graph);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(graph.gameObject.scene);
            Debug.Log($"✅ Đã thêm waypoint mới: {newWP.name}");
        }

        // Nút xóa toàn bộ waypoint
        if (GUILayout.Button("🧹 Clear All Waypoints"))
        {
            if (EditorUtility.DisplayDialog("Xác nhận", "Xóa toàn bộ waypoint?", "OK", "Hủy"))
            {
                // Xóa các object con
                for (int i = graph.transform.childCount - 1; i >= 0; i--)
                    DestroyImmediate(graph.transform.GetChild(i).gameObject);

                // Dọn danh sách waypoint
                if (graph.waypoints != null)
                    graph.waypoints.Clear();

                Debug.Log("🧹 Đã xóa toàn bộ waypoint.");
            }
        }
    }
}
#endif