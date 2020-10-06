using Core.Player.Systems.Inventory;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.UI
{
    public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private CanvasGroup canvasGroup = null;
        private static int inventoryWidth, inventoryHeight = 0;
        private static Inventory inventory;
        
        private void Start()
        {
            if (inventoryWidth == 0)
            {
                inventoryWidth = Inventory.Instance.Width;
                inventoryHeight = Inventory.Instance.Height;
            }
            
            if (inventory == null)
                inventory = Inventory.Instance;
        }
        
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
            bool hitResult = eventData.pointerCurrentRaycast.gameObject.CompareTag("Inventory Slot");
            
            if (hitResult)
            {
                Transform hitTransform = eventData.pointerCurrentRaycast.gameObject.transform;
                transform.position = hitTransform.position;
                int index = hitTransform.GetSiblingIndex();

                int x = index % inventoryWidth;
                int y = index / inventoryWidth;
            }
            
            
            canvasGroup.blocksRaycasts = true;
        }
    }
}
