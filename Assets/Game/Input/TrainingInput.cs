using GameRTS.Commands;
using GameRTS.UI;
using GameRTS.View;
using UnityEngine;

namespace GameRTS.Input
{
    public sealed class TrainingInput : MonoBehaviour
    {
        [SerializeField] private RtsGameView game;
        [SerializeField] private TrainingHudView hud;
        private void OnEnable() { if (hud != null) { hud.TrainRequested += Enqueue; hud.CancelRequested += Cancel; } }
        private void OnDisable() { if (hud != null) { hud.TrainRequested -= Enqueue; hud.CancelRequested -= Cancel; } }
        public void Enqueue() => Submit(TrainingOperation.Enqueue, 0);
        public void Cancel(int ticketId) => Submit(TrainingOperation.Cancel, ticketId);
        private void Submit(TrainingOperation operation, int target)
        {
            if (game == null || game.World == null || game.SelectedTownCenterId <= 0) return;
            game.Submit(new TrainingCommand(game.NextCommandTick, RtsGameView.LocalPlayerId, game.NextLocalSequence(), game.SelectedTownCenterId, operation, target));
        }
    }
}
