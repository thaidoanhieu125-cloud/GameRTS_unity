using System;
using System.Collections.Generic;
using GameRTS.Commands;
using GameRTS.Simulation;
using GameRTS.View;
using UnityEngine;
using UnityEngine.UI;

namespace GameRTS.UI
{
    public sealed class ResearchHudView : MonoBehaviour
    {
        [SerializeField] private RtsGameView game;
        private static ResearchHudView active;
        private RectTransform panel;
        private Text title, status;
        private Button cancel;
        private readonly Dictionary<TechnologyId, Button> buttons = new Dictionary<TechnologyId, Button>();
        private Font font;
        private int sequence = -1;
        private string feedback;
        private float feedbackUntil;
        public event Action<TechnologyId, ResearchOperation> Requested;
        public bool IsVisible => panel != null && panel.gameObject.activeSelf;
        public string StatusText => status != null ? status.text : "";
        public Button GetButton(TechnologyId id) => buttons[id];
        private void Awake()
        {
            active = this; font = GetComponentInChildren<Text>(true).font;
            var go = new GameObject("Research Panel", typeof(RectTransform), typeof(Image)); go.transform.SetParent(transform, false);
            panel = go.GetComponent<RectTransform>(); panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(1, 0);
            panel.anchoredPosition = new Vector2(-16, 48); panel.sizeDelta = new Vector2(390, 270);
            go.GetComponent<Image>().color = new Color(.06f, .11f, .14f, .97f);
            title = Label("Title", -10, 28); title.fontSize = 20;
            status = Label("Status", -163, 54); status.fontSize = 16;
            foreach (TechnologyId id in Enum.GetValues(typeof(TechnologyId)))
            {
                var captured = id; var b = Button(id.ToString(), -48, 50);
                b.onClick.AddListener(() => Requested?.Invoke(captured, ResearchOperation.Start)); buttons.Add(id, b);
            }
            cancel = Button("Cancel research", -222, 34);
            cancel.GetComponentInChildren<Text>().text = "HỦY NGHIÊN CỨU · HOÀN TÀI NGUYÊN";
            cancel.onClick.AddListener(() => { var job = game.World.GetResearch(game.SelectedTrainingBuildingId); if (job != null) Requested?.Invoke(job.Definition.Id, ResearchOperation.Cancel); });
            panel.gameObject.SetActive(false);
        }
        private void OnDestroy() { if (active == this) active = null; }
        public static bool BlocksPointer(Vector2 screen) => active != null && active.IsVisible && RectTransformUtility.RectangleContainsScreenPoint(active.panel, screen);
        private void LateUpdate() => Refresh();
        public void Refresh()
        {
            if (game.World == null) return;
            string type = game.World.ResearchBuildingType(game.SelectedTrainingBuildingId, out int owner);
            bool show = game.World.TechnologiesEnabled && owner == RtsGameView.LocalPlayerId && (type == "town_center" || type == "lumber_camp" || type == "barracks");
            panel.gameObject.SetActive(show); if (!show) return;
            int age = game.World.GetAge(owner); title.text = "CÔNG NGHỆ · ĐỜI " + age + (age == 1 ? " — ĐỒ ĐÁ" : " — ĐỒ ĐỒNG");
            foreach (var b in buttons.Values) b.gameObject.SetActive(false);
            int row = 0;
            foreach (var d in game.World.Technologies)
            {
                if (d.BuildingType != type) continue;
                var b = buttons[d.Id]; b.gameObject.SetActive(true);
                b.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -48 - row++ * 56);
                bool done = game.World.HasTechnology(owner, d.Id);
                b.interactable = !done;
                b.GetComponentInChildren<Text>().text = game.TechnologyName(d.Id) + (done ? " · ĐÃ XONG" : "\n" + Price(d) + " · " + d.Duration / 20 + "s" + (age < d.RequiredAge ? " · CẦN ĐỜI " + d.RequiredAge : ""));
            }
            if (sequence != game.World.LastResearchSequence && game.World.LastResearchPlayer == owner)
            { sequence = game.World.LastResearchSequence; feedback = Describe(game.World.LastResearchIssue); feedbackUntil = Time.unscaledTime + 4; }
            var job = game.World.GetResearch(game.SelectedTrainingBuildingId);
            cancel.gameObject.SetActive(job != null);
            status.text = job != null ? game.TechnologyName(job.Definition.Id) + " · " + job.Percent + "% · Còn " + ((job.Definition.Duration - job.Progress + 19) / 20) + " giây"
                : Time.unscaledTime < feedbackUntil ? feedback : "Chọn công nghệ để nghiên cứu.\nTC nghiên cứu sẽ tạm ngừng nhận huấn luyện.";
        }
        private static string Price(ResearchDefinition d)
        {
            var parts = new List<string>(); if (d.Food > 0) parts.Add(d.Food + " Thực"); if (d.Wood > 0) parts.Add(d.Wood + " Gỗ"); if (d.Gold > 0) parts.Add(d.Gold + " Vàng"); return string.Join(" + ", parts);
        }
        public static string Describe(ResearchIssue issue)
        {
            switch (issue)
            {
                case ResearchIssue.None: return "Đã nhận lệnh.";
                case ResearchIssue.RequiresAge: return "Cần lên Đời 2 trước.";
                case ResearchIssue.Busy: return "Công trình đang huấn luyện/nghiên cứu, hoặc công nghệ đã được nghiên cứu ở nơi khác.";
                case ResearchIssue.AlreadyDone: return "Công nghệ đã hoàn thành.";
                case ResearchIssue.InsufficientResources: return "Không đủ tài nguyên để nghiên cứu.";
                case ResearchIssue.RefundOverflow: return "Ví đầy, chưa thể hoàn tiền; nghiên cứu được giữ lại.";
                default: return "Không thể nghiên cứu tại công trình này.";
            }
        }
        private Text Label(string name, float y, float height)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(panel, false);
            var r = go.GetComponent<RectTransform>(); r.anchorMin = r.anchorMax = r.pivot = new Vector2(.5f, 1); r.anchoredPosition = new Vector2(0, y); r.sizeDelta = new Vector2(370, height);
            var t = go.GetComponent<Text>(); t.font = font; t.color = Color.white; t.alignment = TextAnchor.MiddleCenter; t.raycastTarget = false; return t;
        }
        private Button Button(string name, float y, float height)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(panel, false);
            var r = go.GetComponent<RectTransform>(); r.anchorMin = r.anchorMax = r.pivot = new Vector2(.5f, 1); r.anchoredPosition = new Vector2(0, y); r.sizeDelta = new Vector2(370, height);
            var image = go.GetComponent<Image>(); image.color = new Color(.17f, .32f, .3f);
            var b = go.GetComponent<Button>(); b.targetGraphic = image;
            var text = Label(name + " caption", 0, height); text.transform.SetParent(go.transform, false); text.fontSize = 16;
            return b;
        }
    }
}
