using GameRTS.View;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameRTS.Input
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RtsCameraView), typeof(Camera))]
    public sealed class RtsCameraInput : MonoBehaviour
    {
        [SerializeField] private bool edgeScrolling = true;
        [SerializeField, Min(0f)] private float edgeSize = 16f;

        private RtsCameraView cameraView;
        private Camera viewCamera;

        private void Awake()
        {
            cameraView = GetComponent<RtsCameraView>();
            viewCamera = GetComponent<Camera>();
        }

        private void Update()
        {
            if (!Application.isFocused)
                return;
#if UNITY_EDITOR
            if (UnityEditor.EditorWindow.focusedWindow == null
                || UnityEditor.EditorWindow.focusedWindow.GetType().Name != "GameView")
                return;
#endif

            Vector2 pan = ReadKeyboardPan(Keyboard.current);

            float scroll = 0f;
            Mouse mouse = Mouse.current;
#if UNITY_EDITOR
            // The Input System can retain the last Game View mouse position after leaving it.
            if (UnityEditor.EditorWindow.mouseOverWindow == null
                || UnityEditor.EditorWindow.mouseOverWindow.GetType().Name != "GameView")
                mouse = null;
#endif
            if (mouse != null)
            {
                Vector2 pointer = mouse.position.ReadValue();
                Rect viewport = viewCamera.pixelRect;
                if (viewport.Contains(pointer))
                {
                    scroll = mouse.scroll.ReadValue().y;
                    if (edgeScrolling)
                    {
                        float horizontalEdge = Mathf.Min(edgeSize, viewport.width * 0.5f);
                        float verticalEdge = Mathf.Min(edgeSize, viewport.height * 0.5f);
                        pan.x += (pointer.x >= viewport.xMax - horizontalEdge ? 1f : 0f)
                            - (pointer.x <= viewport.xMin + horizontalEdge ? 1f : 0f);
                        pan.y += (pointer.y >= viewport.yMax - verticalEdge ? 1f : 0f)
                            - (pointer.y <= viewport.yMin + verticalEdge ? 1f : 0f);
                    }
                }
            }

            cameraView.ApplyInput(pan, scroll, Time.unscaledDeltaTime);
        }

        private static Vector2 ReadKeyboardPan(Keyboard keyboard)
        {
            if (keyboard == null)
                return Vector2.zero;

            return new Vector2(
                (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f)
                    - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f),
                (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f)
                    - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f));
        }
    }
}
