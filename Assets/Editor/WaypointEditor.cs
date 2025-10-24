#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Waypoint))]
public class WaypointEditor : Editor
{
    private static Waypoint firstSelected = null;

    private void OnSceneGUI()
    {
        Waypoint current = (Waypoint)target;

        // Vẽ nút nhỏ trên waypoint
        Handles.color = Color.cyan;
        if (Handles.Button(current.transform.position + Vector3.up * 0.25f, Quaternion.identity, 0.2f, 0.2f, Handles.SphereHandleCap))
        {
            if (firstSelected == null)
            {
                firstSelected = current;
                Debug.Log($"🔹 Đã chọn waypoint đầu: {current.name}");
            }
            else if (firstSelected == current)
            {
                Debug.Log("❌ Không thể nối waypoint với chính nó.");
                firstSelected = null;
            }
            else
            {
                // Nối hai waypoint 2 chiều
                if (!firstSelected.neighbors.Contains(current))
                    firstSelected.neighbors.Add(current);

                if (!current.neighbors.Contains(firstSelected))
                    current.neighbors.Add(firstSelected);

                EditorUtility.SetDirty(firstSelected);
                EditorUtility.SetDirty(current);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(current.gameObject.scene);

                Debug.Log($"✅ Đã nối {firstSelected.name} ↔ {current.name}");
                firstSelected = null;
            }
        }

        // Hiển thị các đường nối hiện có
        Handles.color = Color.green;
        foreach (var n in current.neighbors)
        {
            if (n != null)
                Handles.DrawLine(current.Position, n.Position);
        }
    }
}
#endif