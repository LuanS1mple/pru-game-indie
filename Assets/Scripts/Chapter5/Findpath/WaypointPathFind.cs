using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointPathFind : MonoBehaviour
{
    public List<Waypoint> waypoints;

    private void Awake()
    {
        // Thu thập tất cả Waypoint nếu danh sách chưa được điền (tùy chọn)
        if (waypoints == null || waypoints.Count == 0)
        {
            waypoints = FindObjectsOfType<Waypoint>().ToList();
        }
    }

    // Lấy waypoint gần nhất với vị trí
    public Waypoint GetClosestWaypoint(Vector2 position)
    {
        Waypoint closest = null;
        float minDist = float.MaxValue;
        float maxSearchDistance = 10f; // Giới hạn khoảng cách tìm kiếm

        foreach (var wp in waypoints)
        {
            float dist = Vector2.Distance(position, wp.Position);

            // Chỉ xem xét các waypoint trong phạm vi gần
            if (dist < minDist && dist < maxSearchDistance)
            {
                minDist = dist;
                closest = wp;
            }
        }
        return closest;
    }

    // Dùng BFS để kiểm tra kết nối giữa 2 waypoint (Pathfinding - Rất quan trọng cho logic giới hạn)
    public List<Waypoint> FindPath(Waypoint start, Waypoint end)
    {
        if (start == null || end == null)
            return null;

        if (start == end)
        {
            return new List<Waypoint> { start };
        }

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
