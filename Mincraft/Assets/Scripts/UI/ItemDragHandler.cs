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
        private Vector3 localDragStartPosition;

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
            localDragStartPosition = transform.localPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            this.transform.position = eventData.position;
            this.transform.SetAsLastSibling();
        }

        public void OnEndDrag(PointerEventData eventData)
        { 
            bool hitResult = eventData.pointerCurrentRaycast.gameObject != null && eventData.pointerCurrentRaycast.gameObject.CompareTag("Inventory Slot");

            if (hitResult) // Hit something, so inside the grid
            {
                Transform hitTransform = eventData.pointerCurrentRaycast.gameObject.transform;
                transform.position = hitTransform.position;
                int targetIndex = hitTransform.GetSiblingIndex();

                int targetX = targetIndex % inventoryWidth;
                int targetY = targetIndex / inventoryWidth;

                if (inventory[targetX, targetY] != null)
                {
                    //inventory.Swap(multiIndex.X, multiIndex.Y, targetX, targetY);
                }
                else
                {
                    inventory.MoveItem(multiIndex.X, multiIndex.Y, targetX, targetY);
                }
            }
            else // outside the grid
            {
                inventory.Drop(multiIndex.X, multiIndex.Y);
                // ItemData item = inventory[multiIndex.X, multiIndex.Y];
                //
                // for (int i = 0; i < item.Amount; i++)
                // {
                //     GameObject go = droppedItemsManager.GetNextBlock();
                //     
                //     go.transform.position = playerMovementTracker.transform.position + playerMovementTracker.transform.forward;
                //
                //     go.GetComponent<DroppedItemInformation>().FromBlock(new Block((BlockUV) item.ItemID));
                //     go.GetComponent<Rigidbody>().AddForce(playerMovementTracker.transform.forward, ForceMode.Impulse);
                //
                //     droppedItemsManager.AddNewItem(go);
                //     droppedItemsManager.AddBoxColliderHandle(go.transform.GetChild(0).GetComponent<BoxCollider>());
                // }

                // inventoryUI.Delete(multiIndex.X, multiIndex.Y);
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