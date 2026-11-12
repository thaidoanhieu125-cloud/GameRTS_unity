using GameRTS.Data;
using UnityEngine;

namespace GameRTS.View
{
    [DisallowMultipleComponent]
    public sealed class UnitPreviewSpawner : MonoBehaviour
    {
        [SerializeField] private UnitDataSO unitData;
        [SerializeField] private RtsGameView game;
        [SerializeField] private Vector2[] spawnPositions =
        {
            new Vector2(-4f, -1.5f),
            new Vector2(-2f, 1f),
            Vector2.zero,
            new Vector2(2f, 1f),
            new Vector2(4f, -1.5f)
        };
        [SerializeField] private bool selectFirstForPreview = true;

        private void Start()
        {
            if (unitData == null || unitData.ViewPrefab == null || string.IsNullOrWhiteSpace(unitData.UnitId))
            {
                Debug.LogError("Unit preview needs Unit Data with an ID and a view prefab.", this);
                return;
            }

            if (!unitData.ViewPrefab.TryGetComponent(out UnitView template))
            {
                Debug.LogError("The unit view prefab must have UnitView on its root.", this);
                return;
            }

            if (spawnPositions == null)
                return;

            for (int i = 0; i < spawnPositions.Length; i++)
            {
                Vector2 position = spawnPositions[i];
                if (game != null)
                {
                    // In GameScene the Simulation allocates IDs and owns positions.
                    game.SpawnUnit(unitData, position, transform);
                    continue;
                }
                UnitView unit = Instantiate(template, new Vector3(position.x, position.y, 0f),
                    Quaternion.identity, transform);
                unit.name = $"{unitData.DisplayName} {i + 1}";
                unit.Initialize(unitData, i + 1);
                unit.SetSelected(selectFirstForPreview && i == 0);
            }
        }
    }
}
