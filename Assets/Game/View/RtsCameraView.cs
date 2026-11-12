using UnityEngine;
using UnityEngine.Tilemaps;

namespace GameRTS.View
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class RtsCameraView : MonoBehaviour
    {
        [SerializeField] private Tilemap ground;
        [SerializeField, Min(0f)] private float panSpeed = 20f;
        [SerializeField, Min(0f)] private float zoomSensitivity = 1.5f;
        [SerializeField, Min(0.01f)] private float minimumZoom = 4f;
        [SerializeField, Min(0.01f)] private float maximumZoom = 20f;

        private Camera viewCamera;

        public void ApplyInput(Vector2 pan, float scrollDelta, float deltaTime)
        {
            transform.position += (Vector3)(Vector2.ClampMagnitude(pan, 1f) * panSpeed * deltaTime);
            viewCamera.orthographicSize = Mathf.Max(0.01f,
                viewCamera.orthographicSize - scrollDelta * zoomSensitivity);
        }

        private void Awake()
        {
            viewCamera = GetComponent<Camera>();
            viewCamera.orthographic = true;
        }

        private void LateUpdate()
        {
            if (ground == null || ground.cellBounds.size.x == 0 || ground.cellBounds.size.y == 0)
                return;

            Vector3 firstCorner = ground.CellToWorld(ground.cellBounds.min);
            Vector3 lastCorner = ground.CellToWorld(ground.cellBounds.max);
            Vector2 mapMinimum = Vector2.Min(firstCorner, lastCorner);
            Vector2 mapMaximum = Vector2.Max(firstCorner, lastCorner);
            float aspect = Mathf.Max(viewCamera.aspect, 0.01f);
            float fitZoom = Mathf.Min((mapMaximum.y - mapMinimum.y) * 0.5f,
                (mapMaximum.x - mapMinimum.x) / (2f * aspect));
            float upperZoom = Mathf.Min(maximumZoom, fitZoom);
            float lowerZoom = Mathf.Min(minimumZoom, upperZoom);

            viewCamera.orthographicSize = Mathf.Clamp(viewCamera.orthographicSize, lowerZoom, upperZoom);
            float halfHeight = viewCamera.orthographicSize;
            float halfWidth = halfHeight * aspect;
            Vector3 position = transform.position;
            position.x = Mathf.Clamp(position.x, mapMinimum.x + halfWidth, mapMaximum.x - halfWidth);
            position.y = Mathf.Clamp(position.y, mapMinimum.y + halfHeight, mapMaximum.y - halfHeight);
            transform.position = position;
        }

        private void OnValidate()
        {
            minimumZoom = Mathf.Max(0.01f, minimumZoom);
            maximumZoom = Mathf.Max(minimumZoom, maximumZoom);
        }
    }
}
