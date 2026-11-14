using GameRTS.Commands;
using GameRTS.View;
using GameRTS.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameRTS.Input
{
    [DisallowMultipleComponent]
    public sealed class UnitSelectionInput : MonoBehaviour
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField] private RtsGameView game;
        [SerializeField] private UnitSelectionView selection;
        private InputAction leftClick, pointerMotion, rightClick;
        private bool acceptingEvents;
        private bool suppressGesture;

        private void OnEnable()
        {
            if (!Application.isPlaying) return;
            acceptingEvents = true;
            leftClick = new InputAction("Select", InputActionType.Button, "<Mouse>/leftButton");
            pointerMotion = new InputAction("Pointer", InputActionType.PassThrough, "<Mouse>/position");
            rightClick = new InputAction("Move", InputActionType.Button, "<Mouse>/rightButton");
            leftClick.started += OnLeftPressed;
            leftClick.canceled += OnLeftReleased;
            pointerMotion.performed += OnPointerMoved;
            rightClick.performed += OnRightPressed;
            leftClick.Enable(); pointerMotion.Enable(); rightClick.Enable();
        }

        private void Update()
        {
            if (!CanReadMouse() && selection != null) selection.CancelDrag();
        }

        // Capture the position at each input event. Polling only the final mouse state in
        // Update would lose the press origin when a fast drag starts and ends in one frame.
        private void OnLeftPressed(InputAction.CallbackContext context) => HandleMouse(context, true, false, false, false);
        private void OnLeftReleased(InputAction.CallbackContext context) => HandleMouse(context, false, false, true, false);
        private void OnRightPressed(InputAction.CallbackContext context) => HandleMouse(context, false, false, false, true);
        private void OnPointerMoved(InputAction.CallbackContext context)
        {
            var mouse = context.control.device as Mouse;
            if (mouse != null && mouse.leftButton.isPressed) HandleMouse(context, false, true, false, false);
        }

        private void HandleMouse(InputAction.CallbackContext context, bool pressed, bool held, bool released, bool right)
        {
            if (!acceptingEvents) return;
            if (!CanReadMouse()) { selection.CancelDrag(); return; }
            var mouse = context.control.device as Mouse;
            if (mouse == null) return;
            bool additive = Keyboard.current != null
                && (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);
            ProcessPointer(mouse.position.ReadValue(), pressed, held, released, right, additive);
        }
        // Same gesture path is exercised by PlayMode tests without depending on OS focus.
        public void ProcessPointer(Vector2 screen, bool leftPressed, bool leftHeld, bool leftReleased, bool rightPressed, bool additive)
        {
            selection.RefreshSelection();
            if (leftPressed && BuildingPlacementView.BlocksSelection) suppressGesture = true;
            if (BuildingPlacementView.BlocksSelection || suppressGesture)
            { selection.CancelDrag(); if (leftReleased) suppressGesture = false; return; }
            if (EconomyHudView.BlocksPointer(screen)) { selection.CancelDrag(); return; }
            bool inside = viewCamera.pixelRect.Contains(screen);
            if (leftPressed && inside) selection.Begin(screen);
            if (leftHeld) selection.Drag(screen);
            if (leftReleased)
            {
                if (inside) selection.Release(screen, additive);
                else selection.CancelDrag();
            }
            if (inside && rightPressed)
            {
                selection.CancelDrag();
                Vector3 point = viewCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -viewCamera.transform.position.z));
                int x = RtsGameView.ToSimulationCoordinate(point.x), y = RtsGameView.ToSimulationCoordinate(point.y);
                if (game.SelectedTownCenterId > 0)
                {
                    int cell = game.World.Map.CellAt(x, y);
                    if (cell >= 0) game.Submit(new TrainingCommand(game.NextCommandTick, RtsGameView.LocalPlayerId, game.NextLocalSequence(),
                        game.SelectedTownCenterId, TrainingOperation.SetRally, cell));
                    return;
                }
                if (selection.SelectedIds.Count == 0) return;
                if (game.World.TryGetEnemyTarget(RtsGameView.LocalPlayerId, x, y, out var kind, out int targetId))
                {
                    var attackers = new List<int>();
                    foreach (int id in selection.SelectedIds) if (game.World.GetUnit(id).Stats.AttackDamage > 0) attackers.Add(id);
                    if (attackers.Count > 0) game.Submit(new AttackCommand(game.NextCommandTick, RtsGameView.LocalPlayerId, game.NextLocalSequence(), attackers, kind, targetId));
                    return;
                }
                var building = game.World.GetBuildingAt(game.World.Map.CellAt(x, y));
                if (building != null && !building.IsComplete && building.OwnerId == RtsGameView.LocalPlayerId)
                {
                    var builders = new List<int>();
                    foreach (int id in selection.SelectedIds) if (game.World.GetUnit(id)?.Villager != null) builders.Add(id);
                    if (builders.Count > 0)
                    { game.Submit(new ResumeBuildCommand(game.NextCommandTick, RtsGameView.LocalPlayerId, game.NextLocalSequence(), building.Id, builders)); return; }
                }
                var node = game.World.GetResourceAt(game.World.Map.CellAt(x, y));
                if (node != null && !node.IsDepleted)
                {
                    var workers = new List<int>();
                    foreach (int id in selection.SelectedIds)
                        if (game.World.GetUnit(id)?.Villager != null) workers.Add(id);
                    if (workers.Count > 0)
                    {
                        game.Submit(new GatherCommand(game.NextCommandTick, RtsGameView.LocalPlayerId, game.NextLocalSequence(), workers, node.Id));
                        return;
                    }
                }
                game.Submit(new MoveCommand(game.NextCommandTick, RtsGameView.LocalPlayerId, game.NextLocalSequence(),
                    selection.SelectedIds, x, y));
            }
        }

        private static bool CanReadMouse()
        {
            if (!Application.isFocused) return false;
#if UNITY_EDITOR
            if (UnityEditor.EditorWindow.focusedWindow == null
                || UnityEditor.EditorWindow.focusedWindow.GetType().Name != "GameView"
                || UnityEditor.EditorWindow.mouseOverWindow == null
                || UnityEditor.EditorWindow.mouseOverWindow.GetType().Name != "GameView") return false;
#endif
            return true;
        }

        private void OnDisable()
        {
            acceptingEvents = false;
            leftClick?.Dispose(); pointerMotion?.Dispose(); rightClick?.Dispose();
            leftClick = pointerMotion = rightClick = null;
            if (selection != null) selection.CancelDrag();
        }
    }
}
