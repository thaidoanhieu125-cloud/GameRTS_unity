using System;
using System.Globalization;
using GameRTS.Commands;
using GameRTS.Simulation;
using GameRTS.View;
using UnityEngine;
using UnityEngine.UI;

namespace GameRTS.UI
{
    [DisallowMultipleComponent]
    public sealed class EconomyHudView : MonoBehaviour
    {
        [SerializeField] private RtsGameView game;
        [SerializeField] private RectTransform topBar, testPanel;
        [SerializeField] private Text woodValue, foodValue, goldValue, populationValue, feedback, help;
        [SerializeField] private Button toggleButton, grantButton, spendButton, capacityButton;
        private static EconomyHudView active;
        private int displayedSequence = -1;
        private string lastHelp;
        public event Action<EconomyDebugOperation> TestRequested;
        public string WoodText => woodValue.text;
        public string FoodText => foodValue.text;
        public string GoldText => goldValue.text;
        public string PopulationText => populationValue.text;
        public string FeedbackText => feedback.text;
        public RectTransform TopBar => topBar;
        public bool TestPanelOpen => testPanel.gameObject.activeSelf;

        public void Bind(RtsGameView source) => game = source;

        private void Awake()
        {
            active = this;
            toggleButton.onClick.AddListener(ToggleTestPanel);
            grantButton.onClick.AddListener(() => TestRequested?.Invoke(EconomyDebugOperation.GrantResources));
            spendButton.onClick.AddListener(() => TestRequested?.Invoke(EconomyDebugOperation.SpendResources));
            capacityButton.onClick.AddListener(() => TestRequested?.Invoke(EconomyDebugOperation.IncreasePopulationLimit));
            testPanel.gameObject.SetActive(false);
        }

        private void OnDestroy() { if (active == this) active = null; }

        public void ToggleTestPanel()
        {
            if (game != null && game.World.AllowEconomyDebugCommands)
                testPanel.gameObject.SetActive(!TestPanelOpen);
        }

        public static bool BlocksPointer(Vector2 point)
        {
            return active != null && active.isActiveAndEnabled &&
                (RectTransformUtility.RectangleContainsScreenPoint(active.topBar, point)
                || (active.TestPanelOpen && RectTransformUtility.RectangleContainsScreenPoint(active.testPanel, point)));
        }

        private void LateUpdate() => Refresh();

        public void Refresh()
        {
            if (game == null || game.World == null) return;
            PlayerEconomyState economy = game.World.GetPlayerEconomy(RtsGameView.LocalPlayerId);
            if (economy == null) return;
            SetText(woodValue, economy.Wood.ToString(CultureInfo.InvariantCulture));
            SetText(foodValue, economy.Food.ToString(CultureInfo.InvariantCulture));
            SetText(goldValue, economy.Gold.ToString(CultureInfo.InvariantCulture));
            SetText(populationValue, economy.Population + "/" + economy.MaxPopulation);
            populationValue.color = economy.Population >= economy.MaxPopulation ? new Color(1f, .55f, .35f) : Color.white;
            toggleButton.gameObject.SetActive(game.World.AllowEconomyDebugCommands);
            if (!game.World.AllowEconomyDebugCommands) testPanel.gameObject.SetActive(false);
            string instruction = $"Đã chọn: {game.SelectedCount}    •    Trái: chọn / kéo khung    •    Phải: di chuyển    •    WASD: camera    •    Lăn chuột: zoom";
            if (game.World.AllowResourceDebugCommands) instruction += "    •    Giữa/F8: thử nguồn -50 (Shift: hết)";
            if (lastHelp != instruction) { help.text = instruction; lastHelp = instruction; }
            if (game.World.LastEconomyDebugPlayer != RtsGameView.LocalPlayerId
                || displayedSequence == game.World.LastEconomyDebugSequence) return;
            displayedSequence = game.World.LastEconomyDebugSequence;
            bool success = game.World.LastEconomyDebugSucceeded;
            feedback.color = success ? new Color(.55f, .9f, .7f) : new Color(1f, .63f, .43f);
            feedback.text = !success ? "Từ chối: không đủ tài nguyên hoặc vượt giới hạn số."
                : game.World.LastEconomyDebugOperation == EconomyDebugOperation.GrantResources ? "Đã nạp tài nguyên vào ví."
                : game.World.LastEconomyDebugOperation == EconomyDebugOperation.SpendResources ? "Đã thanh toán đủ cả 3 loại tài nguyên."
                : "Đã tăng giới hạn dân số.";
        }

        private static void SetText(Text label, string value) { if (label.text != value) label.text = value; }
    }
}
