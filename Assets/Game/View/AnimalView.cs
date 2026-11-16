using GameRTS.Data;
using GameRTS.Simulation;
using UnityEngine;

namespace GameRTS.View
{
    public sealed class AnimalView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer body;
        [SerializeField] private TextMesh label;
        private AnimalDataSO data;
        private RtsGameView game;
        public UnitState State { get; private set; }
        public Sprite DisplayedSprite => body.sprite;
        public void Initialize(AnimalDataSO definition, UnitState state, RtsGameView source)
        { data = definition; State = state; game = source; Render(); }
        private void LateUpdate() => Render();
        public void Render()
        {
            if (State == null) return;
            var meat = game.World.GetResource(State.Animal.CorpseResourceId);
            bool visible = game.World.IsCellVisible(RtsGameView.LocalPlayerId, game.World.Map.CellAt(State.X, State.Y));
            bool remains = State.IsAlive || meat != null && !meat.IsDepleted;
            body.enabled = visible && remains; label.gameObject.SetActive(visible && remains);
            if (!visible || !remains) return;
            transform.position = new Vector3(State.X / 1000f, State.Y / 1000f, 0);
            body.sprite = State.IsAlive ? data.AliveSprite : data.CarcassSprite;
            label.text = data.DisplayName + " · " + (State.IsAlive ? State.CurrentHP + "/" + State.MaxHP + " HP\n" + State.Animal.Definition.Food : meat.Remaining.ToString()) + " Thực";
        }
    }
}
