using Core.Player;
using Core.Player.Interaction;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.UI
{
    public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler
    {
        private Transform root = null;
        
        private static DroppedItemsManager droppedItemsManager;
        private static PlayerMovementTracker playerMovementTracker;
        private Transform originalParent = null;

        private void Start()
        {
            if (droppedItemsManager == null) 
                droppedItemsManager = DroppedItemsManager.Instance;
            
            if (playerMovementTracker == null) 
                playerMovementTracker = PlayerMovementTracker.Instance;

            root = GameObject.Find("UI").transform;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            SetRaycastBlock(false);
            originalParent = transform.parent;
            transform.parent = root;
        }

        public void OnDrag(PointerEventData eventData)
        {
            this.transform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            
            
            bool inventoryGridHitResult = eventData.pointerCurrentRaycast.gameObject != null &&
                                 eventData.pointerCurrentRaycast.gameObject.CompareTag("Inventory Slot");
            bool quickbarHitResult = eventData.pointerCurrentRaycast.gameObject != null &&
                                     eventData.pointerCurrentRaycast.gameObject.CompareTag("Quick Bar Slot");
            
            
            if (inventoryGridHitResult)
            {
                transform.parent = originalParent;
                SetRaycastBlock(true);
            }
            else if (quickbarHitResult) // Quick bar
            {
                // Bind to quick bar
            }
            else // outside the grid
            {
                //Remove as UI Object
                
                //DropAmountDialog

                //Based on selected amount, remove from inventory
                
                //Drop the actual object with selected amount
                
                Debug.Log("outside the inventory");
            }
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