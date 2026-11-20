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
        public bool IsVisible => gameObject.activeInHierarchy && spriteRenderer != null && spriteRenderer.enabled;
        public bool ContainsVisualPoint(Vector2 point)
        {
            if(!IsVisible || State==null || State.IsDepleted || point.y<transform.position.y+.65f) return false;
            Bounds b=spriteRenderer.bounds;
            return point.x>=b.min.x && point.x<=b.max.x && point.y>=b.min.y && point.y<=b.max.y;
        }

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
#if UNITY_EDITOR
            // OnValidate can run during deserialization; defer renderer changes to an editor update.
            UnityEditor.EditorApplication.delayCall -= RefreshEditorSprite;
            UnityEditor.EditorApplication.delayCall += RefreshEditorSprite;
#endif
        }
#if UNITY_EDITOR
        private void RefreshEditorSprite()
        {
            if(this!=null && data!=null && !Application.isPlaying)
            { var renderer=GetComponent<SpriteRenderer>();if(renderer.sprite!=data.Sprite)renderer.sprite=data.Sprite; }
        }
#endif

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(new Vector3(Mathf.Floor(transform.position.x) + .5f, Mathf.Floor(transform.position.y) + .5f, 0), new Vector3(1, 1, .02f));
        }
    }
}
