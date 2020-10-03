using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.UI
{
    public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private CanvasGroup canvasGroup = null;
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!canvasGroup)
                canvasGroup = GetComponent<CanvasGroup>();

            canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            this.transform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            transform.position = eventData.pointerCurrentRaycast.gameObject.transform.position;
            canvasGroup.blocksRaycasts = true;
        }
    }
}
