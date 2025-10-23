using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WaypointGraph : MonoBehaviour
{
    // Giữ nguyên thuộc tính public này để kéo thả Waypoint đã Instantiate vào.
    public List<Waypoint> waypoints;

    // THUỘC TÍNH MỚI: Để kéo thả Prefab Waypoint mặc định vào Inspector.
    // Điều này chỉ hoạt động khi game KHÔNG chạy.
    [HideInInspector] // Ẩn khỏi Inspector thông thường, chỉ hiện trong Editor tùy chỉnh
    public GameObject waypointPrefabTemplate;

    private void Awake()
    {
        if (waypoints == null || waypoints.Count == 0)
        {
            waypoints = FindObjectsOfType<Waypoint>().ToList();
            // Debug.LogWarning("WaypointGraph: Danh sách Waypoint trống, tự động tìm thấy " + waypoints.Count + " Waypoint trong Scene.");
        }
        else
        {
            // Debug.Log("WaypointGraph: Sử dụng " + waypoints.Count + " Waypoint được thiết lập thủ công.");
        }
    }

    // --- CÁC HÀM LOGIC CHÍNH (Giữ nguyên) ---

    public Waypoint GetClosestWaypoint(Vector2 position)
    {
        Waypoint closest = null;
        float minDist = float.MaxValue;
        float maxSearchDistance = 10f;

        foreach (var wp in waypoints)
        {
            float dist = Vector2.Distance(position, wp.Position);

            if (dist < minDist && dist < maxSearchDistance)
            {
                minDist = dist;
                closest = wp;
            }
        }
        return closest;
    }

    public List<Waypoint> FindPath(Waypoint start, Waypoint end)
    {
        if (start == null || end == null)
            return null;

        if (start == end)
        {
            return new List<Waypoint> { start };
        }

        // Logic BFS (Giữ nguyên)
        Dictionary<Waypoint, Waypoint> cameFrom = new Dictionary<Waypoint, Waypoint>();
        Queue<Waypoint> queue = new Queue<Waypoint>();
        HashSet<Waypoint> visited = new HashSet<Waypoint>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == end)
                return ReconstructPath(cameFrom, start, end);

            foreach (var neighbor in current.neighbors)
            {
                if (neighbor != null && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        return null; // Không tìm thấy đường
    }

    private List<Waypoint> ReconstructPath(Dictionary<Waypoint, Waypoint> cameFrom, Waypoint start, Waypoint end)
    {
        List<Waypoint> path = new List<Waypoint>();
        Waypoint current = end;

        while (current != null && current != start)
        {
            path.Add(current);
            if (!cameFrom.TryGetValue(current, out current))
            {
                return new List<Waypoint>();
            }
        }

        path.Add(start);
        path.Reverse();
        return path;
    }
}