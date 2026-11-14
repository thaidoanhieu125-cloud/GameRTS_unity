using GameRTS.Data;
using GameRTS.Simulation;
using UnityEngine;

namespace GameRTS.View
{
    public sealed class BuildingView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer body;
        [SerializeField] private Transform progressBackground, progressFill;
        [SerializeField] private TextMesh progressLabel;
        [SerializeField] private HealthBarView healthBar;
        [SerializeField] private Transform selectionMarker, rallyMarker;
        private BuildingDataSO data;
        public int InstanceId { get; private set; }
        public string ProgressText => progressLabel.text;
        public Sprite DisplayedSprite => body.sprite;
        public void Initialize(BuildingDataSO definition, BuildingState state, GridMap map)
        {
            data = definition; InstanceId = state.Id;
            transform.position = new Vector3((map.CenterX(state.Cell) + (state.Width - 1) * 500) / 1000f,
                (map.CenterY(state.Cell) + (state.Height - 1) * 500) / 1000f, 0);
            Render(state);
        }
        public void Render(BuildingState state, SimulationWorld world = null, bool selected = false)
        {
            gameObject.SetActive(!state.IsDestroyed);
            if (state.IsDestroyed) return;
            if (selectionMarker != null) selectionMarker.gameObject.SetActive(selected);
            var center = world != null ? world.GetTraining(state.Id) : null;
            if (rallyMarker != null)
            {
                bool flag = selected && center != null && center.RallyCell >= 0; rallyMarker.gameObject.SetActive(flag);
                if (flag) rallyMarker.position = new Vector3(world.Map.CenterX(center.RallyCell) / 1000f, world.Map.CenterY(center.RallyCell) / 1000f, -.2f);
            }
            if (healthBar != null) healthBar.Render(state.HitPoints, state.MaxHitPoints, state.OwnerId == RtsGameView.LocalPlayerId);
            body.sprite = state.IsComplete ? data.CompletedSprite : data.FoundationSprite;
            if (body.sprite != null) body.transform.localScale = new Vector3(state.Width / body.sprite.bounds.size.x, state.Height / body.sprite.bounds.size.y, 1);
            progressBackground.gameObject.SetActive(!state.IsComplete); progressFill.gameObject.SetActive(!state.IsComplete);
            float width = state.Width - .2f, progress = state.ProgressPermille / 1000f, y = -state.Height * .5f - .15f;
            progressBackground.localPosition = new Vector3(0, y, -.1f); progressBackground.localScale = new Vector3(width, .12f, 1);
            progressFill.localPosition = new Vector3(-width * (1 - progress) * .5f, y, -.2f); progressFill.localScale = new Vector3(width * progress, .08f, 1);
            progressLabel.transform.localPosition = new Vector3(0, y - .26f, -.2f);
            progressLabel.text = state.IsComplete ? data.DisplayName : data.DisplayName + " " + state.ProgressPercent + "%";
        }
    }
}
