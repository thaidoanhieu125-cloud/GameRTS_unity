using UnityEngine;

namespace GameRTS.View
{
    public sealed class HealthBarView : MonoBehaviour
    {
        [SerializeField] private Transform fill;
        [SerializeField] private SpriteRenderer fillRenderer;
        [SerializeField] private TextMesh label;
        [SerializeField] private float width = .85f;
        public float Fraction { get; private set; }
        public void Render(int hp, int maximum, bool friendly)
        {
            Fraction = maximum > 0 ? Mathf.Clamp01((float)hp / maximum) : 0;
            fill.localScale = new Vector3(width * Fraction, .07f, 1);
            fill.localPosition = new Vector3(-width * (1 - Fraction) / 2, 0, -.02f);
            fillRenderer.color = friendly ? new Color(.25f, .95f, .35f) : new Color(1, .25f, .2f);
            if (label != null) label.text = hp + "/" + maximum;
        }
    }
}
