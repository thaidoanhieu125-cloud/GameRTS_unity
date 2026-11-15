using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameRTS.UI
{
    public sealed class SceneSwitchView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Text caption;
        [SerializeField] private Button aiButton;
        private static SceneSwitchView active;
        private void Awake()
        {
            active = this;
            bool combat = SceneManager.GetActiveScene().name != "GameScene";
            caption.text = combat ? "VỀ BẢN ĐỒ KINH TẾ" : "THỬ CHIẾN ĐẤU";
            button.onClick.AddListener(() => SceneManager.LoadScene(combat ? "GameScene" : "CombatScene"));
            if (aiButton != null)
            {
                aiButton.gameObject.SetActive(SceneManager.GetActiveScene().name != "AiScene");
                aiButton.onClick.AddListener(() => SceneManager.LoadScene("AiScene"));
            }
        }
        private void OnDestroy() { if (active == this) active = null; }
        public static bool BlocksPointer(Vector2 point) => active != null && active.isActiveAndEnabled
            && (RectTransformUtility.RectangleContainsScreenPoint(active.button.GetComponent<RectTransform>(), point)
                || active.aiButton != null && active.aiButton.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(active.aiButton.GetComponent<RectTransform>(), point));
    }
}
