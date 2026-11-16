using System.Linq;
using GameRTS.Data;
using GameRTS.Simulation;
using UnityEngine;

namespace GameRTS.View
{
    public static class AiMatchOptions { public static AiDifficulty? Difficulty; }
    // Unity supplies match data once. AI decisions subsequently run entirely in Simulation.
    [DefaultExecutionOrder(20)]
    public sealed class AiOpponentView : MonoBehaviour
    {
        [SerializeField] private RtsGameView game;
        [SerializeField] private AiProfileSO profile;
        [SerializeField] private UnitDataSO villager;
        public AiState State => game.World.GetAi(2);
        private void Start()
        {
            foreach (int cell in AiScenario.VillagerCells(game.World.Map, 2))
                game.SpawnUnit(villager, new Vector2(game.World.Map.CenterX(cell) / 1000f, game.World.Map.CenterY(cell) / 1000f), transform, 2);
            game.World.ConfigureVision();
            int town = game.World.DropOffs.Single(d => d.OwnerId == 2 && d.Accepts == ResourceAcceptance.All).Id;
            game.World.AddAiController(2, town, AiMatchOptions.Difficulty.HasValue ? AiDifficulties.Create(AiMatchOptions.Difficulty.Value) : profile.CreateSettings());
        }
    }
}
