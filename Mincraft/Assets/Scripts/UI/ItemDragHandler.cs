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
using Utilities;

namespace Core.UI
{
    public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        // private CanvasGroup canvasGroup = null;
        private static int inventoryWidth, inventoryHeight = 0;
        private static InventoryUI inventoryUI;
        private static DroppedItemsManager droppedItemsManager;
        private static PlayerMovementTracker playerMovementTracker;
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

            if (inventoryUI == null)
                inventoryUI = InventoryUI.Instance;
            
            if (droppedItemsManager == null) 
                droppedItemsManager = DroppedItemsManager.Instance;
            
            if (playerMovementTracker == null) 
                playerMovementTracker = PlayerMovementTracker.Instance;
            
            if (inventory == null) 
                inventory = Inventory.Instance;
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            // if (!canvasGroup)
            //     canvasGroup = GetComponent<CanvasGroup>();

            SetRaycastBlock(false);
            
            multiIndex = inventoryUI.IndexFromGameObject(gameObject);
            localDragStartPosition = transform.localPosition;
            // canvasGroup.blocksRaycasts = false;
        }

        private void Update()
        {
            
        }

        public void OnDrag(PointerEventData eventData)
        {
            this.transform.position = eventData.position;
            this.transform.SetAsLastSibling();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            bool hitResult = eventData.pointerCurrentRaycast.gameObject.CompareTag("Inventory Slot");

            if (hitResult) // Hit something, so inside the grid
            {
                Transform hitTransform = eventData.pointerCurrentRaycast.gameObject.transform;
                transform.position = hitTransform.position;
                int targetIndex = hitTransform.GetSiblingIndex();

                int targetX = targetIndex % inventoryWidth;
                int targetY = targetIndex / inventoryWidth;

                if (inventoryUI.SlotAvailable(targetX, targetY))
                {
                    transform.position = hitTransform.position;
                    inventoryUI.MoveGameObjectFromTo(multiIndex.X, multiIndex.Y, targetX, targetY);
                }
                else // Slot is obviously not available, since there's already on object on it
                {
                    //Moved back to its own position
                    if (multiIndex.X == targetX && multiIndex.Y == targetY)
                    {
                        SetRaycastBlock(true);
                    }
                    else
                    {
                        inventoryUI.Swap(multiIndex.X, multiIndex.Y, targetX, targetY, localDragStartPosition);
                        
                        multiIndex.X = 0;
                        multiIndex.Y = 0;
                    }
                }
            }
            else // outside the grid
            {
                ItemData item = inventory[multiIndex.X, multiIndex.Y];
                
                for (int i = 0; i < item.Amount; i++)
                {
                    GameObject go = droppedItemsManager.GetNextBlock();
                    
                    go.transform.position = playerMovementTracker.transform.position + playerMovementTracker.transform.forward;

                    go.GetComponent<DroppedItemInformation>().FromBlock(new Block((BlockUV) item.ItemID));
                    go.GetComponent<Rigidbody>().AddForce(playerMovementTracker.transform.forward, ForceMode.Impulse);

                    droppedItemsManager.AddNewItem(go);
                    droppedItemsManager.AddBoxColliderHandle(go.transform.GetChild(0).GetComponent<BoxCollider>());
                }

                inventoryUI.Delete(multiIndex.X, multiIndex.Y);
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
