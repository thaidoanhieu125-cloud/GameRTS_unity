using System;
using System.Collections.Generic;
using GameRTS.Commands;

namespace GameRTS.Simulation
{
    public sealed partial class SimulationWorld
    {
        public bool VisionEnabled { get; private set; }
        public int UnitSightCells { get; private set; }
        public int BuildingSightCells { get; private set; }
        private readonly SortedDictionary<int, bool[]> explored = new SortedDictionary<int, bool[]>();

        public void ConfigureVision(int unitSightCells = 8, int buildingSightCells = 10)
        {
            if (Tick != 0 || VisionEnabled || unitSightCells < 1 || unitSightCells > 32 || buildingSightCells < 1 || buildingSightCells > 32)
                throw new InvalidOperationException("Configure sight once before tick 1, with radii 1..32.");
            VisionEnabled = true; UnitSightCells = unitSightCells; BuildingSightCells = buildingSightCells;
            foreach (int player in economies.Keys) explored.Add(player, new bool[Map.Count]);
            UpdateExploration();
        }

        public bool IsCellVisible(int playerId, int cell)
        {
            if (cell < 0 || cell >= Map.Count || GetPlayerEconomy(playerId) == null) return false;
            if (!VisionEnabled) return true;
            int x = Map.CenterX(cell), y = Map.CenterY(cell);
            foreach (var unit in units)
                if (unit.IsAlive && unit.OwnerId == playerId && WithinSight(x, y, unit.X, unit.Y, UnitSightCells)) return true;
            foreach (var depot in dropOffs.Values)
                if (depot.OwnerId == playerId && SeesFromFootprint(x, y, depot.Cell, depot.Width, depot.Height)) return true;
            foreach (var building in buildings.Values)
                if (!building.IsDestroyed && building.IsComplete && building.OwnerId == playerId
                    && SeesFromFootprint(x, y, building.Cell, building.Width, building.Height)) return true;
            return false;
        }

        public bool IsCellExplored(int playerId, int cell) => cell >= 0 && cell < Map.Count
            && (!VisionEnabled || IsCellVisible(playerId, cell) || explored.TryGetValue(playerId, out var cells) && cells[cell]);

        public bool IsFootprintVisible(int playerId, int origin, int width, int height, bool entire = false)
        {
            bool any = false;
            for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
            {
                bool visible = IsCellVisible(playerId, Map.Index(origin % Map.Width + x, origin / Map.Width + y));
                if (entire && !visible) return false;
                any |= visible;
            }
            return any;
        }

        private static bool WithinSight(int x, int y, int sx, int sy, int radius)
        { long dx = (long)x - sx, dy = (long)y - sy, r = radius * 1000L; return dx * dx + dy * dy <= r * r; }
        private bool SeesFromFootprint(int x, int y, int cell, int width, int height)
        {
            int sx = Math.Max(Map.CenterX(cell), Math.Min(Map.CenterX(cell) + (width - 1) * 1000, x));
            int sy = Math.Max(Map.CenterY(cell), Math.Min(Map.CenterY(cell) + (height - 1) * 1000, y));
            return WithinSight(x, y, sx, sy, BuildingSightCells);
        }
        private bool CanSeeBody(int playerId, CombatBody body) => !VisionEnabled
            || IsFootprintVisible(playerId, body.Cell, body.Width, body.Height);
        private void UpdateExploration()
        {
            if (!VisionEnabled) return;
            foreach (var pair in explored)
            {
                foreach (var unit in units) if (unit.IsAlive && unit.OwnerId == pair.Key)
                    Reveal(pair.Value, unit.X, unit.Y, UnitSightCells);
                foreach (var depot in dropOffs.Values) if (depot.OwnerId == pair.Key)
                    RevealFootprint(pair.Value, depot.Cell, depot.Width, depot.Height);
                foreach (var building in buildings.Values) if (building.OwnerId == pair.Key && !building.IsDestroyed && building.IsComplete)
                    RevealFootprint(pair.Value, building.Cell, building.Width, building.Height);
            }
        }
        private void RevealFootprint(bool[] cells, int origin, int width, int height)
        {
            for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
            { int cell = origin + y * Map.Width + x; Reveal(cells, Map.CenterX(cell), Map.CenterY(cell), BuildingSightCells); }
        }
        private void Reveal(bool[] cells, int x, int y, int radius)
        {
            int center = Map.NearestCell(x, y), cx = center % Map.Width, cy = center / Map.Width;
            for (int yy = Math.Max(0, cy - radius); yy <= Math.Min(Map.Height - 1, cy + radius); yy++)
                for (int xx = Math.Max(0, cx - radius); xx <= Math.Min(Map.Width - 1, cx + radius); xx++)
                { int cell = Map.Index(xx, yy); if (WithinSight(Map.CenterX(cell), Map.CenterY(cell), x, y, radius)) cells[cell] = true; }
        }
        private void HashVision(Action<int> add)
        {
            add(VisionEnabled ? 1 : 0); if (!VisionEnabled) return;
            add(UnitSightCells); add(BuildingSightCells); add(explored.Count);
            foreach (var pair in explored) { add(pair.Key); foreach (bool cell in pair.Value) add(cell ? 1 : 0); }
        }
    }
}
