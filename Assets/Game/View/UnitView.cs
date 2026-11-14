using System;
using GameRTS.Data;
using GameRTS.Simulation;
using UnityEngine;

namespace GameRTS.View
{
    [DisallowMultipleComponent]
    public sealed class UnitView : MonoBehaviour
    {
        [SerializeField] private GameObject selectionCircle;
        [SerializeField] private TextMesh cargoLabel;
        [SerializeField] private SpriteRenderer cargoMarker;
        [SerializeField] private Transform hammer;
        [SerializeField] private HealthBarView healthBar;
        [SerializeField] private SpriteRenderer attackFlash;

        public UnitDataSO Data { get; private set; }
        public int InstanceId { get; private set; }
        public bool IsSelected => selectionCircle != null && selectionCircle.activeSelf;

        private void Awake()
        {
            SetSelected(false);
        }

        public void Initialize(UnitDataSO data, int instanceId)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (instanceId <= 0)
                throw new ArgumentOutOfRangeException(nameof(instanceId));

            Data = data;
            InstanceId = instanceId;
            if (selectionCircle != null)
                selectionCircle.transform.localScale = Vector3.one * (data.CollisionRadius * 2f + 0.25f);
            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            if (selectionCircle != null)
                selectionCircle.SetActive(selected);
        }

        public void Render(UnitState state, float alpha, int tick = 0)
        {
            gameObject.SetActive(state.IsAlive);
            if (!state.IsAlive) { SetSelected(false); return; }
            if (healthBar != null) healthBar.Render(state.CurrentHP, state.MaxHP, state.OwnerId == RtsGameView.LocalPlayerId);
            if (attackFlash != null) attackFlash.enabled = state.LastAttackTick >= 0 && tick >= state.LastAttackTick && tick - state.LastAttackTick < 3;
            const float scale = 1f / GridMap.UnitsPerCell;
            transform.position = new Vector3(Mathf.Lerp(state.PreviousX, state.X, alpha) * scale,
                Mathf.Lerp(state.PreviousY, state.Y, alpha) * scale, 0f);
            if (state.Villager != null)
            {
                if (hammer != null)
                {
                    bool working = state.Villager.Activity == VillagerActivity.Building && !state.IsMoving;
                    hammer.gameObject.SetActive(working);
                    if (working) hammer.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.unscaledTime * 16) * 45);
                }
                if (cargoLabel != null) cargoLabel.text = state.Villager.CarriedAmount + "/" + state.Villager.Capacity;
                if (cargoMarker != null)
                {
                    cargoMarker.enabled = state.Villager.CarriedAmount > 0;
                    cargoMarker.color = state.Villager.CarriedType == ResourceType.Wood ? new Color(.65f, .37f, .15f)
                        : state.Villager.CarriedType == ResourceType.Food ? new Color(.95f, .3f, .25f) : new Color(1f, .8f, .2f);
                }
            }
        }
    }
}
