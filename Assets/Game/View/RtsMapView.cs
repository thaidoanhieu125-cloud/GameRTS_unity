using System;
using GameRTS.Simulation;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GameRTS.View
{
    [DisallowMultipleComponent]
    public sealed class RtsMapView : MonoBehaviour
    {
        [SerializeField] private Tilemap ground;
        [SerializeField] private Tilemap obstacles;
        public Tilemap Ground => ground;
        public Tilemap Obstacles => obstacles;

        public GridMap CreateSimulationMap()
        {
            if (ground == null) throw new InvalidOperationException("RtsMapView needs a Ground Tilemap.");
            BoundsInt bounds = ground.cellBounds;
            Vector3 origin = ground.CellToWorld(bounds.min);
            Vector3 right = ground.CellToWorld(bounds.min + Vector3Int.right) - origin;
            Vector3 up = ground.CellToWorld(bounds.min + Vector3Int.up) - origin;
            if ((right - Vector3.right).sqrMagnitude > 0.000001f || (up - Vector3.up).sqrMagnitude > 0.000001f)
                throw new InvalidOperationException("Milestone 1 uses axis-aligned XY tiles of 1 x 1 world unit.");
            var map = new GridMap(bounds.size.x, bounds.size.y,
                RtsGameView.ToSimulationCoordinate(origin.x), RtsGameView.ToSimulationCoordinate(origin.y));
            for (int y = 0; y < map.Height; y++)
            for (int x = 0; x < map.Width; x++)
            {
                var cell = new Vector3Int(bounds.xMin + x, bounds.yMin + y, 0);
                bool blocked = obstacles != null && obstacles.HasTile(
                    obstacles.WorldToCell(ground.GetCellCenterWorld(cell)));
                map.SetWalkable(x, y, ground.HasTile(cell) && !blocked);
            }
            return map;
        }
    }
}
