using GameRTS.Simulation;
using GameRTS.View;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
namespace GameRTS.UI
{
    public sealed class DifficultyMenuView : MonoBehaviour
    {
        private GameObject panel;
        private Text title;
        private Font font;
        public AiDifficulty Selected { get; private set; } = AiDifficulty.Medium;
        public static bool IsOpen { get; private set; }
        public void Open()
        {
            if (panel == null) Build();
            panel.SetActive(true); IsOpen = true; Select(AiDifficulty.Medium);
        }
        public void Select(AiDifficulty difficulty)
        {
            Selected = difficulty;
            title.text = "ĐẤU VỚI MÁY — MAP 1\n" + (difficulty == AiDifficulty.Easy ? "DỄ · 6 quân · nhịp 2 giây" : difficulty == AiDifficulty.Medium
                ? "VỪA · 11 quân · nhịp 1,5 giây" : "KHÓ · 15 quân · hai hướng tấn công");
        }
        public void Begin() { AiMatchOptions.Difficulty=Selected; IsOpen=false; SceneManager.LoadScene("AiScene"); }
        public void Close() { if (panel!=null) panel.SetActive(false); IsOpen=false; }
        private void OnDestroy() { IsOpen=false; }
        private void Build()
        {
            font=GetComponentInChildren<Text>(true).font;
            panel=new GameObject("Difficulty Menu",typeof(RectTransform),typeof(Image)); panel.transform.SetParent(transform,false);
            var rect=panel.GetComponent<RectTransform>(); rect.anchorMin=Vector2.zero; rect.anchorMax=Vector2.one; rect.offsetMin=rect.offsetMax=Vector2.zero;
            panel.GetComponent<Image>().color=new Color(.04f,.08f,.12f,.98f);
            title=Label(panel.transform,"Title",new Vector2(0,125),new Vector2(800,110)); title.fontSize=28;
            Choice("Easy","DỄ",-240,()=>Select(AiDifficulty.Easy));
            Choice("Medium","VỪA",0,()=>Select(AiDifficulty.Medium));
            Choice("Hard","KHÓ",240,()=>Select(AiDifficulty.Hard));
            Choice("Start","BẮT ĐẦU",0,Begin,-100); Choice("Cancel","QUAY LẠI",0,Close,-170);
        }
        private Text Label(Transform parent,string name,Vector2 pos,Vector2 size)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Text)); go.transform.SetParent(parent,false);
            var rect=go.GetComponent<RectTransform>(); rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f); rect.anchoredPosition=pos; rect.sizeDelta=size;
            var t=go.GetComponent<Text>(); t.font=font; t.alignment=TextAnchor.MiddleCenter; t.color=Color.white; t.raycastTarget=false; return t;
        }
        private void Choice(string name,string caption,float x,UnityEngine.Events.UnityAction action,float y=0)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image),typeof(Button)); go.transform.SetParent(panel.transform,false);
            var rect=go.GetComponent<RectTransform>(); rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f); rect.anchoredPosition=new Vector2(x,y); rect.sizeDelta=new Vector2(210,55);
            var image=go.GetComponent<Image>(); image.color=new Color(.16f,.35f,.36f);
            var label=Label(go.transform,"Caption",Vector2.zero,new Vector2(210,55)); label.text=caption; label.fontSize=23;
            var button=go.GetComponent<Button>(); button.targetGraphic=image; button.onClick.AddListener(action);
        }
    }
}
