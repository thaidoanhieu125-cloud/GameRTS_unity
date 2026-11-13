using System;
using GameRTS.Data;
using GameRTS.Simulation;
using UnityEngine;

namespace GameRTS.View
{
    [DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer))]
    public sealed class DropOffView : MonoBehaviour
    {
        [SerializeField] private DropOffDataSO data;
        [SerializeField, Min(1)] private int ownerId = 1;
        public DropOffDataSO Data => data;
        public DropOffState State { get; private set; }
        public void Initialize(SimulationWorld world)
        {
            if (data == null || data.Sprite == null) throw new InvalidOperationException(name + " needs drop-off data and sprite.");
            int sx = RtsGameView.ToSimulationCoordinate(transform.position.x) - data.Width * GridMap.UnitsPerCell / 2;
            int sy = RtsGameView.ToSimulationCoordinate(transform.position.y) - data.Height * GridMap.UnitsPerCell / 2;
            int cell = world.Map.CellAt(sx, sy);
            State = world.AddDropOff(ownerId, cell, data.Width, data.Height, data.Accepts);
            transform.position = new Vector3((world.Map.CenterX(cell) + (data.Width - 1) * 500) / 1000f,
                (world.Map.CenterY(cell) + (data.Height - 1) * 500) / 1000f, 0);
            GetComponent<SpriteRenderer>().sprite = data.Sprite;
        }
        private void OnValidate() { if (data != null) GetComponent<SpriteRenderer>().sprite = data.Sprite; }
    }
}
