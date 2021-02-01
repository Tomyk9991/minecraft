using Core.Math;
using Core.Player;
using Core.Player.Interaction;
using Core.Player.Systems.Inventory;
using Core.UI.Ingame;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.UI
{
    public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        // private CanvasGroup canvasGroup = null;
        private static int inventoryWidth, inventoryHeight = 0;
        private static DroppedItemsManager droppedItemsManager;
        private static PlayerMovementTracker playerMovementTracker;
        private static InventoryUI inventoryUI;
        private static Inventory inventory;
        private Int2 multiIndex;
        private int index;

        private void Start()
        {
            if (inventoryWidth == 0)
            {
                inventoryWidth = Inventory.Instance.Width;
                inventoryHeight = Inventory.Instance.Height;
            }

            if (droppedItemsManager == null) 
                droppedItemsManager = DroppedItemsManager.Instance;
            
            if (playerMovementTracker == null) 
                playerMovementTracker = PlayerMovementTracker.Instance;

            if (inventoryUI == null)
                inventoryUI = InventoryUI.Instance;
            
            if (inventory == null) 
                inventory = Inventory.Instance;
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            SetRaycastBlock(false);
        }

        public void OnDrag(PointerEventData eventData)
        {
            this.transform.position = eventData.position;
            this.transform.SetAsLastSibling();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            bool inventoryGridHitResult = eventData.pointerCurrentRaycast.gameObject != null &&
                                 eventData.pointerCurrentRaycast.gameObject.CompareTag("Inventory Slot");
            bool quickbarHitResult = eventData.pointerCurrentRaycast.gameObject != null &&
                                     eventData.pointerCurrentRaycast.gameObject.CompareTag("Quick Bar Slot");
            
            if (inventoryGridHitResult) // Hit something, so inside the grid
            {
                
            }
            else if (quickbarHitResult) // Quick bar
            {
                
            }
            else // outside the grid
            {
                
            }
            
            SetRaycastBlock(true);
        }

        private void SetRaycastBlock(bool state)
        {
            foreach (Transform child in transform.parent)
            {
                child.GetComponent<CanvasGroup>().blocksRaycasts = state;
            }
        }
    }
}