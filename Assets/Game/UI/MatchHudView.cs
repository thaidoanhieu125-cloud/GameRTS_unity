using GameRTS.Simulation;
using GameRTS.View;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameRTS.UI
{
    public sealed class MatchHudView : MonoBehaviour
    {
        [SerializeField] private RtsGameView game;
        [SerializeField] private GameObject overlay;
        [SerializeField] private Text title;
        [SerializeField] private Button restart;
        private static MatchHudView active;
        public bool IsVisible => overlay.activeSelf;
        public string Title => title.text;
        private void Awake()
        {
            active = this; overlay.SetActive(false);
            restart.onClick.AddListener(() => SceneManager.LoadScene(gameObject.scene.name));
        }
        private void OnDestroy() { if (active == this) active = null; }
        public static bool BlocksPointer => active != null && active.isActiveAndEnabled && active.IsVisible;
        private void LateUpdate() => Refresh();
        public void Refresh()
        {
            bool ended = game != null && game.World != null && game.World.IsMatchOver; overlay.SetActive(ended);
            if (!ended) return;
            MatchOutcome outcome = game.World.GetOutcome(RtsGameView.LocalPlayerId);
            title.text = outcome == MatchOutcome.Victory ? "VICTORY\nCHIẾN THẮNG" : outcome == MatchOutcome.Defeat ? "DEFEAT\nTHẤT BẠI" : "DRAW\nHÒA";
            title.color = outcome == MatchOutcome.Defeat ? new Color(1, .45f, .4f) : new Color(1, .85f, .4f);
        }
    }
}
