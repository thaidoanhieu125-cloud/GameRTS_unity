using System;
using System.Collections.Generic;

namespace GameRTS.Simulation
{
    public sealed class GridMap
    {
        public const int UnitsPerCell = 1000;
        public const int DirectionCount = 8;
        public const int StraightDistance = UnitsPerCell;
        public const int DiagonalDistance = 1414;
        private readonly bool[] walkable;
        private readonly bool[] resourceBlocked;
        public int Width { get; }
        public int Height { get; }
        public int OriginX { get; }
        public int OriginY { get; }
        public int Count => walkable.Length;

        public GridMap(int width, int height, int originX = 0, int originY = 0)
        {
            if (width <= 0 || height <= 0 || width > 256 || height > 256)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (Math.Abs((long)originX) > 1000000 || Math.Abs((long)originY) > 1000000)
                throw new ArgumentOutOfRangeException(nameof(originX));
            Width = width; Height = height; OriginX = originX; OriginY = originY;
            walkable = new bool[width * height];
            resourceBlocked = new bool[width * height];
            for (int i = 0; i < Count; i++) walkable[i] = true;
        }

        public int Index(int x, int y) => x < 0 || y < 0 || x >= Width || y >= Height ? -1 : y * Width + x;
        public bool IsWalkable(int cell) => IsTerrainWalkable(cell) && !resourceBlocked[cell];
        public bool IsTerrainWalkable(int cell) => cell >= 0 && cell < Count && walkable[cell];
        public bool HasResourceBlocker(int cell) => cell >= 0 && cell < Count && resourceBlocked[cell];
        internal void SetResourceBlocked(int cell, bool value) => resourceBlocked[cell] = value;

        // Placement must reject out-of-bounds positions, rather than clamping to the edge.
        public int CellAt(int x, int y)
        {
            long localX = (long)x - OriginX, localY = (long)y - OriginY;
            if (localX < 0 || localY < 0 || localX >= (long)Width * UnitsPerCell || localY >= (long)Height * UnitsPerCell) return -1;
            return Index((int)(localX / UnitsPerCell), (int)(localY / UnitsPerCell));
        }
        public void SetWalkable(int x, int y, bool value)
        {
            int index = Index(x, y);
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(x));
            walkable[index] = value;
        }
        public int CenterX(int cell) => OriginX + (cell % Width) * UnitsPerCell + UnitsPerCell / 2;
        public int CenterY(int cell) => OriginY + (cell / Width) * UnitsPerCell + UnitsPerCell / 2;
        public int NearestCell(int x, int y)
        {
            long localX = (long)x - OriginX, localY = (long)y - OriginY;
            int cx = localX < 0 ? 0 : (int)Math.Min(Width - 1, localX / UnitsPerCell);
            int cy = localY < 0 ? 0 : (int)Math.Min(Height - 1, localY / UnitsPerCell);
            return Index(cx, cy);
        }

        // A fixed neighbour order and cell-index tie break are shared on every machine.
        public int Neighbour(int cell, int direction)
        {
            int x = cell % Width, y = cell / Width;
            switch (direction)
            {
                case 0: return Index(x + 1, y);
                case 1: return Index(x, y + 1);
                case 2: return Index(x - 1, y);
                case 3: return Index(x, y - 1);
                case 4: return Index(x + 1, y + 1);
                case 5: return Index(x - 1, y + 1);
                case 6: return Index(x - 1, y - 1);
                case 7: return Index(x + 1, y - 1);
                default: throw new ArgumentOutOfRangeException(nameof(direction));
            }
        }

        public bool IsDiagonal(int from, int to) => from % Width != to % Width && from / Width != to / Width;
        public int StepDistance(int from, int to) => IsDiagonal(from, to) ? DiagonalDistance : StraightDistance;
        public int SideCellX(int from, int to) => Index(to % Width, from / Width);
        public int SideCellY(int from, int to) => Index(from % Width, to / Width);

        public bool CanTraverse(int from, int to, Func<int, bool> occupied = null)
        {
            if (!IsWalkable(from) || !IsWalkable(to) || (occupied != null && occupied(to))) return false;
            int dx = Math.Abs(from % Width - to % Width), dy = Math.Abs(from / Width - to / Width);
            if (dx > 1 || dy > 1 || dx + dy == 0) return false;
            if (dx == 0 || dy == 0) return true;
            // A diagonal needs both adjacent cardinal cells: never squeeze through a
            // blocked corner, including a corner temporarily occupied by another unit.
            int sideX = SideCellX(from, to), sideY = SideCellY(from, to);
            return IsWalkable(sideX) && IsWalkable(sideY)
                && (occupied == null || (!occupied(sideX) && !occupied(sideY)));
        }

        public bool[] ReachableFrom(int start)
        {
            var reached = new bool[Count];
            if (!IsWalkable(start)) return reached;
            var queue = new Queue<int>(); queue.Enqueue(start); reached[start] = true;
            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                for (int d = 0; d < DirectionCount; d++)
                {
                    int next = Neighbour(current, d);
                    if (next < 0 || reached[next] || !CanTraverse(current, next)) continue;
                    reached[next] = true; queue.Enqueue(next);
                }
            }
            return reached;
        }
    }
}
