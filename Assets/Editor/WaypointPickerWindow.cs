#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class WaypointPickerWindow : EditorWindow
{
    private static Waypoint current;
    private static List<Waypoint> allWaypoints = new List<Waypoint>();

    public static void Open(Waypoint wp)
    {
        current = wp;
        allWaypoints = new List<Waypoint>(FindObjectsOfType<Waypoint>());
        var window = GetWindow<WaypointPickerWindow>("Select Neighbor");
        window.Show();
    }

    private void OnGUI()
    {
        if (current == null)
        {
            Close();
            return;
        }

        EditorGUILayout.LabelField($"Chọn neighbor cho: {current.name}", EditorStyles.boldLabel);
        GUILayout.Space(5);

        foreach (var wp in allWaypoints)
        {
            if (wp == current) continue;

            if (GUILayout.Button(wp.name))
            {
                Undo.RecordObject(current, "Add Neighbor");
                current.neighbors.Add(wp);
                EditorUtility.SetDirty(current);
            }
        }
    }
}
#endif
