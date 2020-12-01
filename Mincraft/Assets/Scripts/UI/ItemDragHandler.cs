using System.Collections.Generic;
using Core.Builder;
using Core.Math;
using Core.Player;
using Core.Player.Interaction;
using Core.Player.Systems.Inventory;
using Core.Saving;
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

            multiIndex = inventory.IndexFromGameObject(gameObject);
            index = inventory.QuickBar.IndexFromGameObject(gameObject);
        }

        public void OnDrag(PointerEventData eventData)
        {
            this.transform.position = eventData.position;
            this.transform.SetAsLastSibling();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            bool gridHitResult = eventData.pointerCurrentRaycast.gameObject != null &&
                                 eventData.pointerCurrentRaycast.gameObject.CompareTag("Inventory Slot");
            bool quickbarHitResult = eventData.pointerCurrentRaycast.gameObject != null &&
                                     eventData.pointerCurrentRaycast.gameObject.CompareTag("Quick Bar Slot");
            
            Int2 invalidMultiIndex = new Int2(-1, -1);
            int invalidIndex = -1;

            if (gridHitResult) // Hit something, so inside the grid
            {
                Transform hitTransform = eventData.pointerCurrentRaycast.gameObject.transform;
                transform.position = hitTransform.position;
                int targetIndex = hitTransform.GetSiblingIndex();

                int targetX = targetIndex % inventoryWidth;
                int targetY = targetIndex / inventoryWidth;

                if (inventory[targetX, targetY] != null)
                {
                    inventory.Swap(multiIndex.X, multiIndex.Y, targetX, targetY);
                }
                else
                {
                    inventory.MoveItem(multiIndex.X, multiIndex.Y, targetX, targetY);
                }
            }
            else if (quickbarHitResult) // Quick bar
            {
                Transform hitTransform = eventData.pointerCurrentRaycast.gameObject.transform;
                transform.position = hitTransform.position;

                int targetIndex = hitTransform.GetSiblingIndex();
                if (inventory.QuickBar[targetIndex] != null)
                {
                    if (multiIndex != invalidMultiIndex)
                    {
                        inventory.QuickBar.Swap(multiIndex, targetIndex);
                    }
                    else
                    {
                        if (index == -1) //Something went wrong
                            Debug.LogError("Index is -1. Something went wrong");

                        inventory.QuickBar.Swap(index, targetIndex);
                    }
                }
                else
                {
                    if (multiIndex != invalidMultiIndex || targetIndex != invalidIndex)
                    {
                        inventory.QuickBar.MoveItem(multiIndex, targetIndex);
                    }
                }
            }
            else // outside the grid
            {
                if (multiIndex != invalidMultiIndex)
                    inventory.Drop(multiIndex.X, multiIndex.Y);
                else
                {
                    if (index == -1)
                        Debug.LogError("Index is -1. Something went wrong");

                    inventory.QuickBar.Drop(index);
                }
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