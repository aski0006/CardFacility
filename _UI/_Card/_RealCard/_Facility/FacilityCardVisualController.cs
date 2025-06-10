using _Card._RealCard._Facility;
using _UI._CommonUIComponents;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _UI._Card._RealCard._Facility {
    public class FacilityCardVisualController : MonoBehaviour {
        [SerializeField] private UIDraggable draggable;
        private BaseFacility _facilityCard;

        public void HandleBeginDrag(PointerEventData data) {
            Debug.Log("拖拽开始");
        }
        public void HandleDragging(PointerEventData data) {
            Debug.Log($"当前拖拽位置: {data.position}");
        }
        public void HandleEndDrag(PointerEventData data) {
            Debug.Log("拖拽结束");
        }
    }
}
