using System;
using System.Collections.Generic;

namespace GameRTS.Simulation
{
    public static class GridPathfinder
    {
        private struct Entry { public int Cell, G, H; }

        // Eight neighbours; diagonal moves require both adjacent cardinal cells to be clear.
        // Returned path excludes start; null means no route, empty means already at goal.
        public static List<int> FindPath(GridMap map, int start, int goal, Func<int, bool> occupied = null)
        {
            if (!map.IsWalkable(start) || !map.IsWalkable(goal) || (occupied != null && occupied(goal)))
                return null;
            var cost = new int[map.Count];
            var parent = new int[map.Count];
            var closed = new bool[map.Count];
            for (int i = 0; i < cost.Length; i++) { cost[i] = int.MaxValue; parent[i] = -1; }
            var heap = new List<Entry>();
            cost[start] = 0;
            Push(heap, new Entry { Cell = start, H = Heuristic(map, start, goal) });
            while (heap.Count > 0)
            {
                Entry entry = Pop(heap);
                int current = entry.Cell;
                if (closed[current] || entry.G != cost[current]) continue;
                if (current == goal)
                {
                    var path = new List<int>();
                    for (int c = goal; c != start; c = parent[c]) path.Add(c);
                    path.Reverse(); return path;
                }
                closed[current] = true;
                for (int d = 0; d < GridMap.DirectionCount; d++)
                {
                    int next = map.Neighbour(current, d);
                    if (next < 0 || closed[next] || !map.CanTraverse(current, next, occupied)) continue;
                    int nextCost = cost[current] + map.StepDistance(current, next);
                    if (nextCost >= cost[next]) continue;
                    cost[next] = nextCost; parent[next] = current;
                    Push(heap, new Entry { Cell = next, G = nextCost, H = Heuristic(map, next, goal) });
                }
            }
            return null;
        }

        private static int Heuristic(GridMap map, int a, int b)
        {
            int dx = Math.Abs(a % map.Width - b % map.Width), dy = Math.Abs(a / map.Width - b / map.Width);
            int diagonals = Math.Min(dx, dy);
            return diagonals * GridMap.DiagonalDistance + (Math.Max(dx, dy) - diagonals) * GridMap.StraightDistance;
        }
        private static bool Before(Entry a, Entry b)
        {
            int f = (a.G + a.H).CompareTo(b.G + b.H);
            if (f != 0) return f < 0;
            return a.H != b.H ? a.H < b.H : a.Cell < b.Cell;
        }
        private static void Push(List<Entry> heap, Entry value)
        {
            heap.Add(value); int i = heap.Count - 1;
            while (i > 0)
            {
                int p = (i - 1) / 2;
                if (!Before(value, heap[p])) break;
                heap[i] = heap[p]; i = p;
            }
            heap[i] = value;
        }
        private static Entry Pop(List<Entry> heap)
        {
            Entry result = heap[0], last = heap[heap.Count - 1]; heap.RemoveAt(heap.Count - 1);
            if (heap.Count == 0) return result;
            int i = 0;
            while (i * 2 + 1 < heap.Count)
            {
                int child = i * 2 + 1;
                if (child + 1 < heap.Count && Before(heap[child + 1], heap[child])) child++;
                if (!Before(heap[child], last)) break;
                heap[i] = heap[child]; i = child;
            }
            heap[i] = last; return result;
        }
    }
}
