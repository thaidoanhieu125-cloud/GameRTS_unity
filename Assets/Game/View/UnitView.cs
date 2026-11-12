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

        public void Render(UnitState state, float alpha)
        {
            const float scale = 1f / GridMap.UnitsPerCell;
            transform.position = new Vector3(Mathf.Lerp(state.PreviousX, state.X, alpha) * scale,
                Mathf.Lerp(state.PreviousY, state.Y, alpha) * scale, 0f);
        }
    }
}
