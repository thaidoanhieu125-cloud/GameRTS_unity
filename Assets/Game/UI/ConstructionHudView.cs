using System;
using GameRTS.Simulation;
using GameRTS.View;
using UnityEngine;
using UnityEngine.UI;

namespace GameRTS.UI
{
    public sealed class ConstructionHudView : MonoBehaviour
    {
        [SerializeField] private RtsGameView game;
        [SerializeField] private BuildingPlacementView placement;
        [SerializeField] private Button buildButton;
        [SerializeField] private RectTransform noticePanel;
        [SerializeField] private Text notice;
        private static ConstructionHudView active;
        private int lastSequence = -1;
        private string message;
        private float messageUntil;
        public event Action BuildRequested;
        public string NoticeText => notice.text;
        private void Awake() { active = this; buildButton.onClick.AddListener(() => BuildRequested?.Invoke()); }
        private void OnDestroy() { if (active == this) active = null; }
        public static bool BlocksPointer(Vector2 screen) => active != null && active.isActiveAndEnabled && active.noticePanel.gameObject.activeSelf
            && RectTransformUtility.RectangleContainsScreenPoint(active.noticePanel, screen);
        public void ShowNotice(string text) { message = text; messageUntil = Time.unscaledTime + 4f; }
        private void LateUpdate()
        {
            if (game == null || placement == null || game.World == null) return;
            if (lastSequence != game.World.LastBuildSequence && game.World.LastBuildPlayer == RtsGameView.LocalPlayerId)
            {
                lastSequence = game.World.LastBuildSequence;
                ShowNotice(game.World.LastBuildIssue == PlacementIssue.None ? "Đã đặt móng. Dân đang tới xây." : "Không đặt được móng: " + Describe(game.World.LastBuildIssue));
            }
            bool placing = placement.IsActive;
            noticePanel.gameObject.SetActive(placing || Time.unscaledTime < messageUntil);
            if (placing)
            {
                var data = placement.Data;
                notice.text = $"{data.DisplayName} {data.Width}×{data.Height} · {data.WoodCost} Gỗ / {data.FoodCost} Thực / {data.GoldCost} Vàng\n"
                    + Describe(placement.Issue) + " · Trái: đặt · Phải/Esc: hủy";
                notice.color = placement.IsValid ? new Color(.6f, 1, .7f) : new Color(1, .65f, .5f);
            }
            else { notice.text = message ?? ""; notice.color = Color.white; }
        }
        public static string Describe(PlacementIssue issue)
        {
            switch (issue)
            {
                case PlacementIssue.None: return "Có thể xây";
                case PlacementIssue.NoBuilders: case PlacementIssue.InvalidBuilder: return "Cần chọn Dân của bạn";
                case PlacementIssue.OutsideMap: return "Đưa con trỏ vào bản đồ";
                case PlacementIssue.Occupied: return "Vướng quân hoặc ô đang được quân giữ";
                case PlacementIssue.BlockedTerrain: return "Vướng cây, công trình hoặc địa hình cấm";
                case PlacementIssue.NoAccess: return "Dân chưa có đường tới cạnh móng";
                case PlacementIssue.InsufficientResources: return "Không đủ tài nguyên";
                case PlacementIssue.PopulationOverflow: return "Vượt giới hạn sức chứa";
                default: return "Dữ liệu công trình không hợp lệ";
            }
        }
    }
}
