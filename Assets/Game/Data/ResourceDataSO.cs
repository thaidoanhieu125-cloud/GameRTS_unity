using GameRTS.Simulation;
using UnityEngine;

namespace GameRTS.Data
{
    [CreateAssetMenu(fileName = "ResourceData", menuName = "GameRTS/Resource Data")]
    public sealed class ResourceDataSO : ScriptableObject
    {
        [SerializeField] private string resourceId = "tree";
        [SerializeField] private string displayName = "Cây";
        [SerializeField] private ResourceType resourceType;
        [SerializeField, Min(1)] private int initialAmount = 100;
        [SerializeField] private Sprite sprite;
        public string ResourceId => resourceId;
        public string DisplayName => displayName;
        public ResourceType ResourceType => resourceType;
        public int InitialAmount => initialAmount;
        public Sprite Sprite => sprite;
        private void OnValidate()
        {
            resourceId = resourceId?.Trim(); displayName = displayName?.Trim();
            initialAmount = Mathf.Max(1, initialAmount);
        }
    }
}
