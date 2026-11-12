using GameRTS.Commands;
using GameRTS.View;
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
        private int sequence;
        private InputAction leftClick, pointerMotion, rightClick;
        private bool acceptingEvents;

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
                if (selection.SelectedIds.Count == 0) return;
                Vector3 point = viewCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -viewCamera.transform.position.z));
                game.Submit(new MoveCommand(game.NextCommandTick, RtsGameView.LocalPlayerId, ++sequence,
                    selection.SelectedIds, RtsGameView.ToSimulationCoordinate(point.x), RtsGameView.ToSimulationCoordinate(point.y)));
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
