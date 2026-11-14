using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameRTS.UI
{
    public sealed class SceneSwitchView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Text caption;
        private static SceneSwitchView active;
        private void Awake()
        {
            active = this;
            bool combat = SceneManager.GetActiveScene().name == "CombatScene";
            caption.text = combat ? "VỀ BẢN ĐỒ KINH TẾ" : "THỬ CHIẾN ĐẤU";
            button.onClick.AddListener(() => SceneManager.LoadScene(combat ? "GameScene" : "CombatScene"));
        }
        private void OnDestroy() { if (active == this) active = null; }
        public static bool BlocksPointer(Vector2 point) => active != null && active.isActiveAndEnabled
            && RectTransformUtility.RectangleContainsScreenPoint(active.button.GetComponent<RectTransform>(), point);
    }
}
