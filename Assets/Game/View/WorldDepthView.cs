using UnityEngine;
using UnityEngine.Rendering;

namespace GameRTS.View
{
    [ExecuteAlways, DisallowMultipleComponent, RequireComponent(typeof(SortingGroup))]
    public sealed class WorldDepthView : MonoBehaviour
    {
        [SerializeField] private float footOffset;
        private SortingGroup group;
        private void OnEnable() => LateUpdate();
        private void LateUpdate()
        {
            if(group==null) group=GetComponent<SortingGroup>();
            group.sortingOrder=1000-Mathf.RoundToInt((transform.position.y+footOffset)*10f);
        }
    }
}
