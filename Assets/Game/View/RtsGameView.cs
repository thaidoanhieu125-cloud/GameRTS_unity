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
        [Header("Starting economy (each preview unit uses 1 population)")]
        [SerializeField, Min(0)] private int startingWood = 200;
        [SerializeField, Min(0)] private int startingFood = 200;
        [SerializeField, Min(0)] private int startingGold = 100;
        [SerializeField, Min(0)] private int startingMaxPopulation = 10;
        [SerializeField] private bool enableEconomyTesting = true;
        [SerializeField] private bool enableVictoryCondition;
        [SerializeField] private int[] additionalPlayers = Array.Empty<int>();
        [SerializeField] private ProjectileView arrowPrefab;
        private readonly Dictionary<int, ProjectileView> arrowViews = new Dictionary<int, ProjectileView>();
        private readonly Stack<ProjectileView> arrowPool = new Stack<ProjectileView>();
        public int VisibleProjectileCount => arrowViews.Count;
        [Header("Resource placements loaded before units spawn")]
        [SerializeField] private Transform resourceRoot;
        [SerializeField] private bool enableResourceTesting = true;
        private ResourceNodeView[] resourceViews = Array.Empty<ResourceNodeView>();
        public IReadOnlyList<ResourceNodeView> ResourceViews => resourceViews;
        [SerializeField] private Transform dropOffRoot;
        private DropOffView[] dropOffViews = Array.Empty<DropOffView>();
        public IReadOnlyList<DropOffView> DropOffViews => dropOffViews;
        [SerializeField] private BuildingDataSO[] buildingCatalog;
        [SerializeField] private Transform constructedBuildingRoot;
        private readonly Dictionary<int, BuildingView> buildingViews = new Dictionary<int, BuildingView>();
        public IReadOnlyDictionary<int, BuildingView> BuildingViews => buildingViews;
        public BuildingDataSO GetBuildingData(string typeId)
        {
            if (buildingCatalog != null) foreach (var data in buildingCatalog) if (data != null && data.TypeId == typeId) return data;
            return null;
        }
        private readonly List<UnitView> units = new List<UnitView>();
        private readonly Dictionary<string, UnitDataSO> unitDataById = new Dictionary<string, UnitDataSO>(StringComparer.Ordinal);
        public UnitDataSO GetUnitData(string typeId) => unitDataById.TryGetValue(typeId, out UnitDataSO data) ? data : null;
        private int localSequence;
        private double accumulatedTime;
        public const int LocalPlayerId = 1;
        public SimulationWorld World { get; private set; }
        public IReadOnlyList<UnitView> Units => units;
        public int NextCommandTick => World.Tick + 2;
        public int SelectedCount { get; set; }
        public int SelectedTrainingBuildingId { get; set; }
        public int NextLocalSequence() => ++localSequence;
        public static int ToSimulationCoordinate(float value) => Mathf.RoundToInt(value * GridMap.UnitsPerCell);

        private void Awake()
        {
            if (mapView == null) throw new InvalidOperationException("RtsGameView needs a map.");
            World = new SimulationWorld(mapView.CreateSimulationMap(), enableEconomyTesting, enableResourceTesting);
            World.AddPlayer(LocalPlayerId, startingWood, startingFood, startingGold, startingMaxPopulation);
            foreach (int player in additionalPlayers) World.AddPlayer(player);
            if (buildingCatalog != null) foreach (var data in buildingCatalog) if (data != null)
            {
                World.RegisterBuildingDefinition(data.CreateDefinition());
                if (data.TrainingData != null) unitDataById[data.TrainingData.UnitData.UnitId] = data.TrainingData.UnitData;
            }
            if (resourceRoot != null)
            {
                resourceViews = resourceRoot.GetComponentsInChildren<ResourceNodeView>();
                foreach (ResourceNodeView resource in resourceViews) resource.Initialize(World);
            }
            if (dropOffRoot != null)
            {
                dropOffViews = dropOffRoot.GetComponentsInChildren<DropOffView>();
                foreach (DropOffView depot in dropOffViews)
                {
                    depot.Initialize(World);
                    if (depot.TrainingData != null) unitDataById[depot.TrainingData.UnitData.UnitId] = depot.TrainingData.UnitData;
                }
            }
            ConfigureVictory();
        }

        private void ConfigureVictory()
        {
            if (!enableVictoryCondition) return;
            int own = 0, enemy = 0;
            foreach (var depot in dropOffViews) if (depot.State.OwnerId == LocalPlayerId) own = depot.State.Id; else enemy = depot.State.Id;
            World.ConfigureTownCenterVictory(own, enemy);
        }
        public UnitView SpawnUnit(UnitDataSO data, Vector2 position, Transform parent, int ownerId = LocalPlayerId)
        {
            unitDataById[data.UnitId] = data;
            UnitState state = World.AddUnit(data.UnitId, ownerId,
                ToSimulationCoordinate(data.MoveSpeed), ToSimulationCoordinate(data.CollisionRadius),
                ToSimulationCoordinate(position.x), ToSimulationCoordinate(position.y), data.CarryCapacity, data.GatherIntervalTicks, data.CreateCombatStats());
            return CreateUnitView(data, state, parent);
        }
        private UnitView CreateUnitView(UnitDataSO data, UnitState state, Transform parent)
        {
            UnitView view = Instantiate(data.ViewPrefab.GetComponent<UnitView>(), parent);
            view.name = $"{data.DisplayName} {state.Id}";
            view.Initialize(data, state.Id);
            view.Render(state, 1f, World.Tick);
            units.Add(view); return view;
        }

        public bool Submit(ISimulationCommand command) => World.Submit(command);

        private void Update()
        {
            if (World.IsMatchOver) { accumulatedTime = 0; return; }
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
            RenderUnits(alpha);
            foreach (ResourceNodeView resource in resourceViews) resource.Render();
            foreach (DropOffView depot in dropOffViews) depot.Render(World, SelectedTrainingBuildingId == depot.State.Id);
            RenderBuildings(); RenderProjectiles(alpha);
        }
        public void RenderUnits(float alpha = 1f)
        {
            while (units.Count < World.Units.Count)
            {
                UnitState state = World.Units[units.Count]; UnitDataSO data = GetUnitData(state.TypeId);
                if (data == null) throw new InvalidOperationException("Missing View data for trained unit " + state.TypeId);
                CreateUnitView(data, state, units.Count > 0 ? units[0].transform.parent : transform);
            }
            foreach (UnitView view in units) view.Render(World.GetUnit(view.InstanceId), alpha, World.Tick);
        }
        public void RenderBuildings()
        {
            foreach (BuildingState state in World.Buildings)
            {
                if (!buildingViews.TryGetValue(state.Id, out BuildingView view))
                {
                    BuildingDataSO data = GetBuildingData(state.Definition.TypeId);
                    view = Instantiate(data.ViewPrefab.GetComponent<BuildingView>(), constructedBuildingRoot != null ? constructedBuildingRoot : transform);
                    view.Initialize(data, state, World.Map); buildingViews.Add(state.Id, view);
                }
                view.Render(state, World, SelectedTrainingBuildingId == state.Id);
            }
        }

        public void RenderProjectiles(float alpha = 1)
        {
            if (arrowPrefab == null) return;
            var alive = new HashSet<int>();
            foreach (var arrow in World.Projectiles)
            {
                alive.Add(arrow.Id);
                if (!arrowViews.TryGetValue(arrow.Id, out var view))
                {
                    view = arrowPool.Count > 0 ? arrowPool.Pop() : Instantiate(arrowPrefab, transform);
                    view.gameObject.SetActive(true); arrowViews.Add(arrow.Id, view);
                }
                view.Render(arrow, alpha);
            }
            var expired = new List<int>(); foreach (int id in arrowViews.Keys) if (!alive.Contains(id)) expired.Add(id);
            foreach (int id in expired) { var view = arrowViews[id]; view.gameObject.SetActive(false); arrowPool.Push(view); arrowViews.Remove(id); }
        }

    }
}
