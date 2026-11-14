using GameRTS.Simulation;
using UnityEngine;

namespace GameRTS.View
{
    public sealed class ProjectileView : MonoBehaviour
    {
        public void Render(ProjectileState arrow, float alpha)
        {
            transform.position = new Vector3(Mathf.Lerp(arrow.PreviousX, arrow.X, alpha) / 1000f, Mathf.Lerp(arrow.PreviousY, arrow.Y, alpha) / 1000f, -.3f);
            int dx = arrow.X - arrow.PreviousX, dy = arrow.Y - arrow.PreviousY;
            if (dx != 0 || dy != 0) transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dy, dx) * Mathf.Rad2Deg);
        }
    }
}
