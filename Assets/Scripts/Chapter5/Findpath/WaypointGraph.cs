using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WaypointGraph : MonoBehaviour
{
    public List<Waypoint> waypoints;

    // Lấy waypoint gần nhất với vị trí
    public Waypoint GetClosestWaypoint(Vector2 position)
    {
        Waypoint closest = null;
        float minDist = float.MaxValue;

        foreach (var wp in waypoints)
        {
            float dist = Vector2.Distance(position, wp.Position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = wp;
            }
        }
        return closest;
    }

    // 👉 Hàm này để EnemyPathFollower.cs có thể gọi
    public Waypoint GetClosestNode(Vector2 position)
    {
        return GetClosestWaypoint(position);
    }

    // Kiểm tra xem 2 waypoint có nằm trong cùng khu vực (connected region)
    public bool AreInSameRegion(Waypoint a, Waypoint b)
    {
        if (a == null || b == null) return false;

        HashSet<Waypoint> visited = new HashSet<Waypoint>();
        Queue<Waypoint> queue = new Queue<Waypoint>();
        queue.Enqueue(a);
        visited.Add(a);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == b)
                return true;

            foreach (var n in current.neighbors)
            {
                if (n != null && !visited.Contains(n))
                {
                    visited.Add(n);
                    queue.Enqueue(n);
                }
            }
        }
        return false;
    }

    // 👉 Thêm hàm FindPath cho EnemyPathFollower.cs
    // Dùng BFS để tìm đường đi ngắn nhất giữa 2 waypoint
    public List<Waypoint> FindPath(Waypoint start, Waypoint end)
    {
        if (start == null || end == null)
            return null;

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

        return null; // không tìm thấy đường
    }

    private List<Waypoint> ReconstructPath(Dictionary<Waypoint, Waypoint> cameFrom, Waypoint start, Waypoint end)
    {
        List<Waypoint> path = new List<Waypoint>();
        Waypoint current = end;

        while (current != null)
        {
            path.Add(current);
            cameFrom.TryGetValue(current, out current);
        }

        path.Reverse();
        return path;
    }
}
