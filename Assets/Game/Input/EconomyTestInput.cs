using GameRTS.Commands;
using GameRTS.UI;
using GameRTS.View;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameRTS.Input
{
    // Development controls for the wallet. Actual economy gameplay is added in later steps.
    public sealed class EconomyTestInput : MonoBehaviour
    {
        [SerializeField] private RtsGameView game;
        [SerializeField] private EconomyHudView hud;
        private InputAction toggle;
        private void OnEnable()
        {
            if (!Application.isPlaying) return;
            if (hud != null) hud.TestRequested += Request;
            toggle = new InputAction("Toggle wallet test", InputActionType.Button, "<Keyboard>/f9");
            toggle.performed += Toggle;
            toggle.Enable();
        }
        private void OnDisable()
        {
            if (hud != null) hud.TestRequested -= Request;
            toggle?.Dispose(); toggle = null;
        }

        private void Toggle(InputAction.CallbackContext context)
        {
            if (!Application.isFocused) return;
#if UNITY_EDITOR
            if (UnityEditor.EditorWindow.focusedWindow == null || UnityEditor.EditorWindow.focusedWindow.GetType().Name != "GameView") return;
#endif
            hud.ToggleTestPanel();
        }

        public void Request(EconomyDebugOperation operation)
        {
            if (game == null || !game.World.AllowEconomyDebugCommands) return;
            int amount = operation == EconomyDebugOperation.IncreasePopulationLimit ? 5 : 50;
            game.Submit(new EconomyDebugCommand(game.NextCommandTick, RtsGameView.LocalPlayerId,
                game.NextLocalSequence(), operation, amount));
        }
    }
}
