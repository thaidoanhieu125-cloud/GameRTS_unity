using System.Collections.Generic;
using GameRTS.Commands;
using GameRTS.UI;
using GameRTS.View;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameRTS.Input
{
    [DefaultExecutionOrder(100)]
    public sealed class BuildingPlacementInput : MonoBehaviour
    {
        [SerializeField] private RtsGameView game;
        [SerializeField] private UnitSelectionView selection;
        [SerializeField] private BuildingPlacementView placement;
        [SerializeField] private ConstructionHudView hud;
        private InputAction build, confirm, cancel, escape;
        private void OnEnable()
        {
            if (!Application.isPlaying) return;
            if (hud != null) hud.BuildRequested += TogglePlacement;
            build = new InputAction("Build house", InputActionType.Button, "<Keyboard>/b");
            confirm = new InputAction("Place foundation", InputActionType.Button, "<Mouse>/leftButton");
            cancel = new InputAction("Cancel foundation", InputActionType.Button, "<Mouse>/rightButton");
            escape = new InputAction("Cancel building mode", InputActionType.Button, "<Keyboard>/escape");
            build.performed += _ => { if (HasFocus()) TogglePlacement(); };
            confirm.started += _ => { if (HasFocus() && Mouse.current != null && placement.IsActive) ConfirmAtScreen(Mouse.current.position.ReadValue()); };
            cancel.performed += _ => { if (HasFocus() && placement.IsActive) CancelPlacement(); };
            escape.performed += _ => { if (HasFocus() && placement.IsActive) CancelPlacement(); };
            build.Enable(); confirm.Enable(); cancel.Enable(); escape.Enable();
        }
        private void OnDisable()
        {
            if (hud != null) hud.BuildRequested -= TogglePlacement;
            build?.Dispose(); confirm?.Dispose(); cancel?.Dispose(); escape?.Dispose();
            if (placement != null && placement.IsActive) placement.Hide();
        }
        private static bool HasFocus()
        {
            if (!Application.isFocused) return false;
#if UNITY_EDITOR
            if (UnityEditor.EditorWindow.focusedWindow == null || UnityEditor.EditorWindow.focusedWindow.GetType().Name != "GameView") return false;
#endif
            return true;
        }
        private List<int> Builders()
        {
            var ids = new List<int>();
            foreach (int id in selection.SelectedIds) if (game.World.GetUnit(id)?.Villager != null) ids.Add(id);
            return ids;
        }
        public void TogglePlacement()
        {
            if (placement.IsActive) CancelPlacement(); else BeginPlacement();
        }
        public bool BeginPlacement()
        {
            if (Builders().Count == 0) { hud.ShowNotice("Chọn Dân trước khi xây nhà."); return false; }
            var data = game.GetBuildingData("house");
            if (data == null) { hud.ShowNotice("Chưa có dữ liệu Nhà."); return false; }
            selection.CancelDrag(); placement.Begin(data); return true;
        }
        public void CancelPlacement() { placement.Hide(); hud.ShowNotice("Đã hủy chế độ đặt móng."); }
        public void PreviewAtScreen(Vector2 screen) => placement.Evaluate(screen, Builders(), EconomyHudView.BlocksPointer(screen));
        public bool ConfirmAtScreen(Vector2 screen)
        {
            if (!placement.IsActive) return false;
            PreviewAtScreen(screen);
            if (!placement.IsValid) return false;
            bool submitted = game.Submit(new BuildCommand(game.NextCommandTick, RtsGameView.LocalPlayerId, game.NextLocalSequence(), placement.Data.TypeId, placement.Cell, Builders()));
            if (submitted) { placement.Hide(); hud.ShowNotice("Đã gửi lệnh đặt móng."); }
            return submitted;
        }
        private void LateUpdate()
        {
            if (placement.IsActive && HasFocus() && Mouse.current != null) PreviewAtScreen(Mouse.current.position.ReadValue());
        }
    }
}
