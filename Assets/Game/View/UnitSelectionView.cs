using System.Collections.Generic;
using UnityEngine;

namespace GameRTS.View
{
    [DisallowMultipleComponent]
    public sealed class UnitSelectionView : MonoBehaviour
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField] private RtsGameView game;
        [SerializeField, Min(1f)] private float dragThreshold = 6f;
        private Vector2 origin, pointer;
        private bool dragging, crossedThreshold;
        private readonly List<int> selectedIds = new List<int>();
        public IReadOnlyList<int> SelectedIds => selectedIds;
        public bool IsBoxSelecting => dragging && crossedThreshold;

        public void Begin(Vector2 screen)
        {
            origin = pointer = screen; dragging = true; crossedThreshold = false;
        }
        public void Drag(Vector2 screen)
        {
            if (!dragging) return;
            pointer = screen;
            crossedThreshold |= (pointer - origin).sqrMagnitude >= dragThreshold * dragThreshold;
        }
        public void Release(Vector2 screen, bool additive = false)
        {
            if (!dragging) return;
            Drag(screen);
            if (crossedThreshold) SelectRectangle(Rect.MinMaxRect(Mathf.Min(origin.x, screen.x),
                Mathf.Min(origin.y, screen.y), Mathf.Max(origin.x, screen.x), Mathf.Max(origin.y, screen.y)), additive);
            else SelectAt(screen, additive);
            CancelDrag();
        }
        public void CancelDrag() { dragging = false; crossedThreshold = false; }

        public void Clear()
        {
            foreach (UnitView unit in game.Units) unit.SetSelected(false);
            selectedIds.Clear(); game.SelectedCount = 0;
            game.SelectedTrainingBuildingId = 0;
        }

        public void SelectAt(Vector2 screen, bool additive = false)
        {
            UnitView hit = null; float best = float.PositiveInfinity;
            Vector3 world = viewCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -viewCamera.transform.position.z));
            foreach (UnitView unit in game.Units)
            {
                var state = game.World.GetUnit(unit.InstanceId);
                if (state == null || !state.IsAlive || state.OwnerId != RtsGameView.LocalPlayerId) continue;
                Vector3 projected = viewCamera.WorldToScreenPoint(unit.transform.position);
                float distance = ((Vector2)unit.transform.position - (Vector2)world).sqrMagnitude;
                float radius = Mathf.Max(unit.Data.CollisionRadius, 0.3f);
                if (projected.z > 0f && distance <= radius * radius && distance < best)
                { best = distance; hit = unit; }
            }
            if (!additive) Clear();
            if (hit != null) SetSelected(hit, !additive || !hit.IsSelected);
            else
            {
                var center = game.World.GetTrainingAt(game.World.Map.CellAt(RtsGameView.ToSimulationCoordinate(world.x), RtsGameView.ToSimulationCoordinate(world.y)));
                if (center != null && center.OwnerId == RtsGameView.LocalPlayerId) { Clear(); game.SelectedTrainingBuildingId = center.BuildingId; }
                else
                {
                    var b = game.World.GetBuildingAt(game.World.Map.CellAt(RtsGameView.ToSimulationCoordinate(world.x), RtsGameView.ToSimulationCoordinate(world.y)));
                    if (b != null && b.OwnerId == RtsGameView.LocalPlayerId && b.IsComplete && !b.IsDestroyed) { Clear(); game.SelectedTrainingBuildingId = b.Id; }
                }
            }
        }

        public void SelectRectangle(Rect screenRect, bool additive = false)
        {
            game.SelectedTrainingBuildingId = 0;
            if (!additive) Clear();
            foreach (UnitView unit in game.Units)
            {
                var state = game.World.GetUnit(unit.InstanceId);
                if (state == null || !state.IsAlive || state.OwnerId != RtsGameView.LocalPlayerId) continue;
                Vector3 position = viewCamera.WorldToScreenPoint(unit.transform.position);
                if (position.z > 0f && screenRect.Contains(position)) SetSelected(unit, true);
            }
        }

        private void SetSelected(UnitView unit, bool selected)
        {
            game.SelectedTrainingBuildingId = 0;
            unit.SetSelected(selected);
            if (selected && !selectedIds.Contains(unit.InstanceId)) selectedIds.Add(unit.InstanceId);
            else if (!selected) selectedIds.Remove(unit.InstanceId);
            selectedIds.Sort(); game.SelectedCount = selectedIds.Count;
        }

        public void RefreshSelection()
        {
            for (int i = selectedIds.Count - 1; i >= 0; i--)
            {
                var state = game.World.GetUnit(selectedIds[i]);
                if (state == null || !state.IsAlive || state.OwnerId != RtsGameView.LocalPlayerId) selectedIds.RemoveAt(i);
            }
            game.SelectedCount = selectedIds.Count;
            if (game.SelectedTrainingBuildingId > 0 && game.World.GetTraining(game.SelectedTrainingBuildingId) == null)
            {
                var b = game.World.GetBuilding(game.SelectedTrainingBuildingId);
                if (b == null || b.IsDestroyed || !b.IsComplete) game.SelectedTrainingBuildingId = 0;
            }
        }
        private void LateUpdate() => RefreshSelection();
        private void OnGUI()
        {
            if (!IsBoxSelecting) return;
            Rect rect = Rect.MinMaxRect(Mathf.Min(origin.x, pointer.x), Screen.height - Mathf.Max(origin.y, pointer.y),
                Mathf.Max(origin.x, pointer.x), Screen.height - Mathf.Min(origin.y, pointer.y));
            Color before = GUI.color;
            GUI.color = new Color(0.3f, 1f, 0.35f, 0.15f); GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = new Color(0.3f, 1f, 0.35f, 0.95f);
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - 2, rect.width, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.y, 2, rect.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.xMax - 2, rect.y, 2, rect.height), Texture2D.whiteTexture);
            GUI.color = before;
        }
    }
}
