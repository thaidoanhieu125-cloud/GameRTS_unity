using GameRTS.Commands;
using GameRTS.Simulation;
using GameRTS.UI;
using GameRTS.View;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameRTS.Input
{
    public sealed class ResourceTestInput : MonoBehaviour
    {
        [SerializeField] private RtsGameView game;
        [SerializeField] private Camera viewCamera;
        private InputAction extract;
        private void OnEnable()
        {
            if (!Application.isPlaying) return;
            extract = new InputAction("Test resource stock", InputActionType.Button, "<Keyboard>/f8");
            extract.AddBinding("<Mouse>/middleButton");
            extract.performed += OnExtract; extract.Enable();
        }
        private void OnDisable() { extract?.Dispose(); extract = null; }
        private void OnExtract(InputAction.CallbackContext context)
        {
            if (!Application.isFocused || Mouse.current == null) return;
#if UNITY_EDITOR
            if (UnityEditor.EditorWindow.focusedWindow == null || UnityEditor.EditorWindow.focusedWindow.GetType().Name != "GameView") return;
#endif
            bool all = Keyboard.current != null && (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);
            RequestAtScreen(Mouse.current.position.ReadValue(), all);
        }

        public bool RequestAtScreen(Vector2 screen, bool depleteAll = false)
        {
            if (BuildingPlacementView.BlocksSelection || game == null || !game.World.AllowResourceDebugCommands || !viewCamera.pixelRect.Contains(screen)
                || EconomyHudView.BlocksPointer(screen)) return false;
            Vector3 point = viewCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -viewCamera.transform.position.z));
            int cell = game.World.Map.CellAt(RtsGameView.ToSimulationCoordinate(point.x), RtsGameView.ToSimulationCoordinate(point.y));
            ResourceNodeState node = game.World.GetResourceAt(cell);
            if (node == null || node.IsDepleted) return false;
            return game.Submit(new ResourceDebugCommand(game.NextCommandTick, RtsGameView.LocalPlayerId,
                game.NextLocalSequence(), node.Id, depleteAll ? int.MaxValue : 50));
        }
    }
}
