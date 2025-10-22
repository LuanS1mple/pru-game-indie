using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Waypoint : MonoBehaviour
{
    [Tooltip("Danh sách các waypoint lân cận mà node này có thể đi tới")]
    public List<Waypoint> neighbors = new List<Waypoint>();

    public Vector2 Position => transform.position;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.12f);

        Gizmos.color = Color.green;
        foreach (var n in neighbors)
        {
            if (n != null)
                Gizmos.DrawLine(transform.position, n.transform.position);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Handles.Label(transform.position + Vector3.up * 0.3f, name);
    }

    private void OnValidate()
    {
        // Tự động nối 2 chiều khi bạn thêm 1 node vào neighbor
        foreach (var n in neighbors)
        {
            if (n != null && !n.neighbors.Contains(this))
            {
                n.neighbors.Add(this);
                EditorUtility.SetDirty(n);
            }
        }
    }
#endif
}
