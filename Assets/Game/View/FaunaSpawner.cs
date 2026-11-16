using UnityEngine;
using GameRTS.Data;

namespace GameRTS.View
{
    [DefaultExecutionOrder(30)]
    public sealed class FaunaSpawner : MonoBehaviour
    {
        [SerializeField] private RtsGameView game;
        [SerializeField] private AnimalDataSO[] data;
        [SerializeField] private Vector2[] positions;
        private void Start()
        {
            if (data == null || positions == null || data.Length != positions.Length) throw new System.InvalidOperationException("Animal data/positions mismatch.");
            for (int i = 0; i < data.Length; i++)
            {
                int cell = game.World.Map.CellAt(RtsGameView.ToSimulationCoordinate(positions[i].x), RtsGameView.ToSimulationCoordinate(positions[i].y));
                var state = game.World.AddAnimal(data[i].CreateDefinition(), cell);
                var view = Instantiate(data[i].ViewPrefab.GetComponent<AnimalView>(), transform);
                view.name = data[i].DisplayName + " " + state.Id; view.Initialize(data[i], state, game);
            }
        }
    }
}
