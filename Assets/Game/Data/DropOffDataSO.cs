using GameRTS.Simulation;
using UnityEngine;

namespace GameRTS.Data
{
    [CreateAssetMenu(fileName = "DropOffData", menuName = "GameRTS/Drop Off Data")]
    public sealed class DropOffDataSO : ScriptableObject
    {
        [SerializeField] private string displayName = "Tòa thị chính";
        [SerializeField, Range(1, 8)] private int width = 2;
        [SerializeField, Range(1, 8)] private int height = 2;
        [SerializeField] private ResourceAcceptance accepts = ResourceAcceptance.All;
        [SerializeField] private Sprite sprite;
        [SerializeField] private Sprite bronzeSprite;
        public Sprite BronzeSprite => bronzeSprite;
        [SerializeField, Min(1)] private int maxHP = 1000;
        [SerializeField, Min(0)] private int armor = 2;
        public string DisplayName => displayName;
        public int Width => width;
        public int Height => height;
        public ResourceAcceptance Accepts => accepts;
        public Sprite Sprite => sprite;
        public int MaxHP => maxHP;
        public int Armor => armor;
        private void OnValidate() { width = Mathf.Clamp(width, 1, 8); height = Mathf.Clamp(height, 1, 8); }
    }
}
