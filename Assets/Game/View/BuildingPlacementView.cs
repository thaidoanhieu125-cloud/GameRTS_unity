using System.Collections.Generic;
using GameRTS.Data;
using GameRTS.Simulation;
using UnityEngine;

namespace GameRTS.View
{
    public sealed class BuildingPlacementView : MonoBehaviour
    {
        [SerializeField] private RtsGameView game;
        [SerializeField] private Camera viewCamera;
        [SerializeField] private Sprite cellSprite;
        [SerializeField] private Material spriteMaterial;
        private static BuildingPlacementView current;
        private static int suppressThroughFrame = -1;
        private GameObject ghost;
        private SpriteRenderer body;
        private readonly List<SpriteRenderer> cells = new List<SpriteRenderer>();
        public bool IsActive { get; private set; }
        public BuildingDataSO Data { get; private set; }
        public int Cell { get; private set; } = -1;
        public PlacementIssue Issue { get; private set; }
        public bool IsValid => IsActive && Cell >= 0 && Issue == PlacementIssue.None;
        public Color PreviewColor => body != null ? body.color : Color.clear;
        public static bool BlocksSelection => (current != null && current.IsActive) || Time.frameCount <= suppressThroughFrame;
        private void Awake() { current = this; suppressThroughFrame = -1; }
        private void OnDestroy() { if (current == this) current = null; if (ghost != null) Destroy(ghost); }
        public void Begin(BuildingDataSO data)
        {
            if (ghost != null) Destroy(ghost);
            cells.Clear(); Data = data; IsActive = true; Cell = -1; Issue = PlacementIssue.OutsideMap;
            ghost = new GameObject("Building Ghost"); ghost.transform.SetParent(transform, false);
            var spriteObject = new GameObject("Preview", typeof(SpriteRenderer)); spriteObject.transform.SetParent(ghost.transform, false);
            body = spriteObject.GetComponent<SpriteRenderer>(); body.sprite = data.CompletedSprite; body.sharedMaterial = spriteMaterial; body.sortingOrder = 26;
            if (body.sprite != null) body.transform.localScale = new Vector3(data.Width / body.sprite.bounds.size.x, data.Height / body.sprite.bounds.size.y, 1);
            for (int y = 0; y < data.Height; y++) for (int x = 0; x < data.Width; x++)
            {
                var obj = new GameObject("Footprint Cell", typeof(SpriteRenderer)); obj.transform.SetParent(ghost.transform, false);
                obj.transform.localPosition = new Vector3(x + .5f - data.Width * .5f, y + .5f - data.Height * .5f, 0);
                obj.transform.localScale = new Vector3(.94f, .94f, 1);
                var renderer = obj.GetComponent<SpriteRenderer>(); renderer.sprite = cellSprite; renderer.sharedMaterial = spriteMaterial; renderer.sortingOrder = 25;
                cells.Add(renderer);
            }
            ghost.SetActive(false);
        }
        public void Evaluate(Vector2 screen, IReadOnlyList<int> builders, bool pointerBlocked)
        {
            if (!IsActive) return;
            if (pointerBlocked || !viewCamera.pixelRect.Contains(screen))
            { Cell = -1; Issue = PlacementIssue.OutsideMap; ghost.SetActive(false); return; }
            Vector3 point = viewCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -viewCamera.transform.position.z));
            int cell = game.World.Map.CellAt(RtsGameView.ToSimulationCoordinate(point.x), RtsGameView.ToSimulationCoordinate(point.y));
            Cell = cell; Issue = game.World.CheckBuildingPlacement(RtsGameView.LocalPlayerId, Data.TypeId, cell, builders);
            ghost.transform.position = new Vector3(Mathf.Floor(point.x) + Data.Width * .5f, Mathf.Floor(point.y) + Data.Height * .5f, 0);
            body.color = IsValid ? new Color(.15f, 1, .4f, .55f) : new Color(1, .2f, .15f, .55f);
            foreach (var renderer in cells) renderer.color = IsValid ? new Color(.1f, 1, .3f, .25f) : new Color(1, .1f, .1f, .25f);
            ghost.SetActive(true);
        }
        public void Hide()
        {
            IsActive = false; suppressThroughFrame = Time.frameCount + 1;
            if (ghost != null) ghost.SetActive(false);
        }
    }
}
