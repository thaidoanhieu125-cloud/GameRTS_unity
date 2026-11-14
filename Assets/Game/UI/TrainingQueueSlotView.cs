using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameRTS.UI
{
    public sealed class TrainingQueueSlotView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image icon;
        [SerializeField] private Text order;
        public int TicketId { get; private set; }
        public event Action<int> CancelRequested;
        public void Render(int ticketId, Sprite sprite, int position)
        {
            TicketId = ticketId; icon.sprite = sprite; icon.enabled = ticketId > 0;
            order.text = ticketId > 0 ? (position + 1).ToString() : "";
        }
        public void OnPointerClick(PointerEventData data)
        { if (data.button == PointerEventData.InputButton.Right && TicketId > 0) CancelRequested?.Invoke(TicketId); }
    }
}
