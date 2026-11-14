using System;
using GameRTS.Simulation;
using GameRTS.View;
using UnityEngine;
using UnityEngine.UI;

namespace GameRTS.UI
{
    public sealed class TrainingHudView : MonoBehaviour
    {
        [SerializeField] private RtsGameView game;
        [SerializeField] private RectTransform panel;
        [SerializeField] private Button trainButton;
        [SerializeField] private Image progress;
        [SerializeField] private Text progressText, queueText, status;
        [SerializeField] private TrainingQueueSlotView[] slots;
        private static TrainingHudView active;
        private int lastSequence = -1;
        private string feedback;
        private float feedbackUntil;
        public event Action TrainRequested;
        public event Action<int> CancelRequested;
        public bool IsVisible => panel.gameObject.activeSelf;
        public string StatusText => status.text;
        public string QueueText => queueText.text;
        public float Progress => progress.fillAmount;
        private void Awake()
        {
            active = this; trainButton.onClick.AddListener(() => TrainRequested?.Invoke());
            foreach (var slot in slots) slot.CancelRequested += id => CancelRequested?.Invoke(id);
            panel.gameObject.SetActive(false);
        }
        private void OnDestroy() { if (active == this) active = null; }
        public static bool BlocksPointer(Vector2 point) => active != null && active.isActiveAndEnabled && active.IsVisible
            && RectTransformUtility.RectangleContainsScreenPoint(active.panel, point);
        private void LateUpdate() => Refresh();
        public void Refresh()
        {
            if (game == null || game.World == null) return;
            TrainingState center = game.World.GetTraining(game.SelectedTownCenterId);
            panel.gameObject.SetActive(center != null);
            if (center == null) return;
            if (lastSequence != game.World.LastTrainingSequence && game.World.LastTrainingPlayer == RtsGameView.LocalPlayerId)
            {
                lastSequence = game.World.LastTrainingSequence; feedback = Describe(game.World.LastTrainingIssue);
                feedbackUntil = Time.unscaledTime + 4;
            }
            int queued = game.World.QueuedPopulation(center.OwnerId);
            queueText.text = $"Queue: [{center.Queue.Count}/{center.Definition.QueueLimit}] · Đặt chỗ: {queued}";
            progress.fillAmount = center.Queue.Count == 0 ? 0 : (float)center.Queue[0].ProgressTicks / center.Definition.DurationTicks;
            progressText.text = center.ProgressPercent + "%";
            var data = game.GetUnitData(center.Definition.UnitTypeId);
            Sprite icon = data != null ? data.ViewPrefab.GetComponent<SpriteRenderer>().sprite : null;
            for (int i = 0; i < slots.Length; i++) slots[i].Render(i < center.Queue.Count ? center.Queue[i].Id : 0, icon, i);
            status.text = center.WaitingForSpawn ? "Đã đủ 100% · Chờ ô trống sát Tòa thị chính."
                : Time.unscaledTime < feedbackUntil && !string.IsNullOrEmpty(feedback) ? feedback
                : "Chuột phải icon hàng đợi: hủy và hoàn Thực.";
        }
        public static string Describe(TrainingIssue issue)
        {
            switch (issue)
            {
                case TrainingIssue.None: return "";
                case TrainingIssue.PopulationLimit: return "Cần xây thêm Nhà dân";
                case TrainingIssue.QueueFull: return "Hàng đợi đã đủ 5 Dân.";
                case TrainingIssue.NotEnoughFood: return "Không đủ Thực để huấn luyện Dân.";
                case TrainingIssue.InvalidTicket: return "Lượt này đã hoàn thành hoặc đã hủy.";
                case TrainingIssue.RefundOverflow: return "Ví Thực đã đầy, chưa thể hoàn tiền; lượt được giữ nguyên.";
                default: return "Lệnh huấn luyện / tập kết không hợp lệ.";
            }
        }
    }
}
