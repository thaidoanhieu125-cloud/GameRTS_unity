using GameRTS.Simulation;
using UnityEngine;

namespace GameRTS.Data
{
    [CreateAssetMenu(menuName = "GameRTS/Animal Data")]
    public sealed class AnimalDataSO : ScriptableObject
    {
        [SerializeField] private string typeId = "deer", displayName = "Nai";
        [SerializeField] private AnimalKind kind;
        [SerializeField, Min(1)] private int food = 250, maxHP = 32;
        [SerializeField, Min(0)] private int armor, damage;
        [SerializeField, Min(1)] private int speed = 4000, attackInterval = 20;
        [SerializeField] private Sprite aliveSprite, carcassSprite;
        [SerializeField] private GameObject viewPrefab;
        public string DisplayName => displayName;
        public Sprite AliveSprite => aliveSprite;
        public Sprite CarcassSprite => carcassSprite;
        public GameObject ViewPrefab => viewPrefab;
        public AnimalDefinition CreateDefinition() => new AnimalDefinition(typeId, kind, food, speed, new CombatStats(maxHP, armor, damage, attackInterval));
    }
}
