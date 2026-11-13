using System;
using GameRTS.Data;
using GameRTS.Simulation;
using UnityEngine;

namespace GameRTS.View
{
    [DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer))]
    public sealed class ResourceNodeView : MonoBehaviour
    {
        [SerializeField] private ResourceDataSO data;
        [SerializeField] private TextMesh stockLabel;
        private SpriteRenderer spriteRenderer;
        public ResourceDataSO Data => data;
        public ResourceNodeState State { get; private set; }
        public bool IsVisible => spriteRenderer != null && spriteRenderer.enabled;

        public void Initialize(SimulationWorld world)
        {
            if (data == null || data.Sprite == null) throw new InvalidOperationException(name + " needs ResourceData and a sprite.");
            int cell = world.Map.CellAt(RtsGameView.ToSimulationCoordinate(transform.position.x), RtsGameView.ToSimulationCoordinate(transform.position.y));
            State = world.AddResource(data.ResourceId, data.ResourceType, data.InitialAmount, cell);
            transform.position = new Vector3(world.Map.CenterX(cell) / 1000f, world.Map.CenterY(cell) / 1000f, 0);
            spriteRenderer = GetComponent<SpriteRenderer>(); spriteRenderer.sprite = data.Sprite;
            Render();
        }

        public void Render()
        {
            if (State == null) return;
            spriteRenderer.enabled = !State.IsDepleted;
            if (stockLabel != null)
            {
                stockLabel.gameObject.SetActive(!State.IsDepleted);
                string amount = State.Remaining.ToString(System.Globalization.CultureInfo.InvariantCulture);
                if (stockLabel.text != amount) stockLabel.text = amount;
            }
        }

        private void OnValidate()
        {
            if (data != null) GetComponent<SpriteRenderer>().sprite = data.Sprite;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(new Vector3(Mathf.Floor(transform.position.x) + .5f, Mathf.Floor(transform.position.y) + .5f, 0), new Vector3(1, 1, .02f));
        }
    }
}
