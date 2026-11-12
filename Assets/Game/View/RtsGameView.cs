using System;
using System.Collections.Generic;
using GameRTS.Commands;
using GameRTS.Data;
using GameRTS.Simulation;
using UnityEngine;

namespace GameRTS.View
{
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    public sealed class RtsGameView : MonoBehaviour
    {
        [SerializeField] private RtsMapView mapView;
        private readonly List<UnitView> units = new List<UnitView>();
        private double accumulatedTime;
        public const int LocalPlayerId = 1;
        public SimulationWorld World { get; private set; }
        public IReadOnlyList<UnitView> Units => units;
        public int NextCommandTick => World.Tick + 2;
        public int SelectedCount { get; set; }
        public static int ToSimulationCoordinate(float value) => Mathf.RoundToInt(value * GridMap.UnitsPerCell);

        private void Awake()
        {
            if (mapView == null) throw new InvalidOperationException("RtsGameView needs a map.");
            World = new SimulationWorld(mapView.CreateSimulationMap());
        }

        public UnitView SpawnUnit(UnitDataSO data, Vector2 position, Transform parent)
        {
            UnitState state = World.AddUnit(data.UnitId, LocalPlayerId,
                ToSimulationCoordinate(data.MoveSpeed), ToSimulationCoordinate(data.CollisionRadius),
                ToSimulationCoordinate(position.x), ToSimulationCoordinate(position.y));
            UnitView view = Instantiate(data.ViewPrefab.GetComponent<UnitView>(), parent);
            view.name = $"{data.DisplayName} {state.Id}";
            view.Initialize(data, state.Id);
            view.Render(state, 1f);
            units.Add(view); return view;
        }

        public bool Submit(MoveCommand command) => World.Submit(command);

        private void Update()
        {
            accumulatedTime += Time.unscaledDeltaTime;
            double duration = 1.0 / SimulationWorld.TickRate;
            int steps = 0;
            // A slow frame keeps its backlog. Tick count and state never depend on dropping time.
            while (accumulatedTime >= duration && steps++ < 8)
            {
                World.Step(); accumulatedTime -= duration;
            }
        }

        private void LateUpdate()
        {
            float alpha = Mathf.Clamp01((float)(accumulatedTime * SimulationWorld.TickRate));
            foreach (UnitView view in units) view.Render(World.GetUnit(view.InstanceId), alpha);
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(12, 12, 450, 64), GUIContent.none);
            GUI.Label(new Rect(24, 20, 425, 24), "Chuột trái: chọn / kéo khung   |   Chuột phải: di chuyển");
            GUI.Label(new Rect(24, 44, 425, 24), $"Đã chọn: {SelectedCount}   |   WASD: camera   |   Con lăn: zoom");
        }
    }
}
