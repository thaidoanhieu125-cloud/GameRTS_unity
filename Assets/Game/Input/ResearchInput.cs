using GameRTS.Commands;
using GameRTS.UI;
using GameRTS.View;
using UnityEngine;

namespace GameRTS.Input
{
    public sealed class ResearchInput : MonoBehaviour
    {
        [SerializeField] private RtsGameView game;
        [SerializeField] private ResearchHudView hud;
        private void OnEnable() { if (Application.isPlaying && hud != null) hud.Requested += Send; }
        private void OnDisable() { if (hud != null) hud.Requested -= Send; }
        private void Send(TechnologyId id, ResearchOperation operation)
        {
            if (game.SelectedTrainingBuildingId <= 0) return;
            game.Submit(new ResearchCommand(game.NextCommandTick, RtsGameView.LocalPlayerId, game.NextLocalSequence(), game.SelectedTrainingBuildingId, id, operation));
        }
    }
}
