using GameRTS.Data;
using UnityEngine;

namespace GameRTS.View
{
    // Loaded only by CombatScene, after the three economic Villagers have spawned.
    [DefaultExecutionOrder(10)]
    public sealed class CombatPreviewSpawner : MonoBehaviour
    {
        [SerializeField] private RtsGameView game;
        [SerializeField] private UnitDataSO melee, archer;
        private void Start()
        {
            game.SpawnUnit(melee, new Vector2(1.5f, 4.5f), transform);
            game.SpawnUnit(archer, new Vector2(1.5f, 2.5f), transform);
            game.SpawnUnit(melee, new Vector2(10.5f, 2.5f), transform, 2);
            game.SpawnUnit(archer, new Vector2(12.5f, 4.5f), transform, 2);
        }
    }
}
