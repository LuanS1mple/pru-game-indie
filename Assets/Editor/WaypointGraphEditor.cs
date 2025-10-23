using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

// Đặt Editor Script này để tùy chỉnh giao diện của WaypointGraph
[CustomEditor(typeof(WaypointGraph))]
public class WaypointGraphEditor : Editor
{
    private WaypointGraph graph;

    private void OnEnable()
    {
        graph = (WaypointGraph)target;
    }

    public override void OnInspectorGUI()
    {
        // 1. Vẽ các thuộc tính mặc định
        DrawDefaultInspector();

        // 2. TẠO VÙNG KÉO THẢ PREFAB MẶC ĐỊNH
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("=== Waypoint Tools ===", EditorStyles.boldLabel);

        // Ô kéo thả Prefab Waypoint (sử dụng thuộc tính mới)
        graph.waypointPrefabTemplate = (GameObject)EditorGUILayout.ObjectField(
            "Default Waypoint Prefab",
            graph.waypointPrefabTemplate,
            typeof(GameObject),
            false
        );

        // 3. Button Tạo Waypoint
        if (GUILayout.Button("Add New Waypoint"))
        {
            AddNewWaypoint();
        }

        // 4. Button Xóa (Hữu ích để dọn dẹp)
        if (GUILayout.Button("Clear All Waypoints"))
        {
            ClearAllWaypoints();
        }

        // Áp dụng thay đổi vào script
        if (GUI.changed)
        {
            EditorUtility.SetDirty(graph);
        }
    }

    void AddNewWaypoint()
    {
        if (graph.waypointPrefabTemplate == null)
        {
            Debug.LogError("Vui lòng kéo Prefab Waypoint vào ô 'Default Waypoint Prefab' trước.");
            return;
        }

        // --- Logic tạo Waypoint từ Prefab ---

        // 1. Tạo instance của Prefab
        GameObject newWaypointGO = (GameObject)PrefabUtility.InstantiatePrefab(graph.waypointPrefabTemplate);

        // 2. Đặt vị trí
        Vector3 spawnPosition = Vector3.zero;
        if (SceneView.lastActiveSceneView != null)
        {
            // Đặt Waypoint tại vị trí trung tâm của Scene View
            spawnPosition = SceneView.lastActiveSceneView.pivot;
            spawnPosition.z = graph.transform.position.z; // Giữ nguyên Z
        }
        else
        {
            // Nếu không có Scene View, đặt tại vị trí của Graph Manager
            spawnPosition = graph.transform.position;
        }

        newWaypointGO.transform.position = spawnPosition;
        newWaypointGO.transform.SetParent(graph.transform); // Đặt Waypoint mới làm con của WaypointGraph

        Waypoint newWaypoint = newWaypointGO.GetComponent<Waypoint>();

        if (newWaypoint != null)
        {
            // 3. Thêm Waypoint mới vào danh sách
            if (graph.waypoints == null)
            {
                graph.waypoints = new List<Waypoint>();
            }
            graph.waypoints.Add(newWaypoint);

            // Tự động chọn đối tượng mới tạo để người dùng có thể dễ dàng di chuyển nó
            Selection.activeObject = newWaypointGO;

            // Đánh dấu graph là "Dirty" để lưu thay đổi
            EditorUtility.SetDirty(graph);
        }
        else
        {
            DestroyImmediate(newWaypointGO);
            Debug.LogError("Prefab không chứa component Waypoint!");
        }
    }

    void ClearAllWaypoints()
    {
        if (EditorUtility.DisplayDialog("Cảnh báo!", "Bạn có chắc muốn xóa TẤT CẢ Waypoint trong danh sách này không?", "Xóa", "Hủy"))
        {
            // Xóa các đối tượng trong Scene
            foreach (var wp in graph.waypoints)
            {
                if (wp != null)
                {
                    DestroyImmediate(wp.gameObject);
                }
            }

            // Xóa danh sách
            graph.waypoints.Clear();
            EditorUtility.SetDirty(graph);
        }
    }
}