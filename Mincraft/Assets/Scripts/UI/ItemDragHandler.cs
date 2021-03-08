using System.Linq;
using Core.Builder;
using Core.Player;
using Core.Player.Interaction;
using Core.Player.Systems.Inventory;
using Core.Saving;
using Core.UI.Ingame;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.UI
{
    public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler
    {
        private static Transform root = null;

        private static DroppedItemsManager droppedItemsManager;
        private static PlayerMovementTracker playerMovementTracker;
        private static Inventory inventory = null;
        private static QuickBar quickBar = null;
        private int quickbarHitIndex = -1;


        private static Transform inventorySlotsParent = null;

        private void Start()
        {
            if (droppedItemsManager == null) droppedItemsManager = DroppedItemsManager.Instance;
            if (playerMovementTracker == null) playerMovementTracker = PlayerMovementTracker.Instance;
            if (inventory == null) inventory = Inventory.Instance;
            if (quickBar == null) quickBar = inventory.QuickBar;

            if(root == null) root = GameObject.Find("UI").transform;
            if (inventorySlotsParent == null)
            {
                inventorySlotsParent = Resources.FindObjectsOfTypeAll<GameObject>()
                    .FirstOrDefault((GameObject g) => g.name == "Inventory Slots")
                    .transform;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            var data = gameObject.GetComponent<UIItemDataHolder>().Data;
            ReleaseQuickbarIndexIfDoable(data);
            
            SetRaycastBlock(false);
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
            

            if (inventoryGridHitResult) // Inventory
            {
                transform.parent = inventorySlotsParent;
                SetRaycastBlock(true);
                
                var data = gameObject.GetComponent<UIItemDataHolder>().Data;
                ReleaseQuickbarIndexIfDoable(data);
            }
            else if (quickbarHitResult) // Quick bar
            {
                quickbarHitIndex = eventData.pointerCurrentRaycast.gameObject.transform.GetSiblingIndex();

                if (quickBar[quickbarHitIndex] != null)
                {
                    transform.parent = inventorySlotsParent;
                    SetRaycastBlock(true);
                    return;
                }

                var data = gameObject.GetComponent<UIItemDataHolder>().Data;
                quickBar[quickbarHitIndex] = data;
                data.QuickbarIndex = quickbarHitIndex;
                
                this.transform.position = eventData.pointerCurrentRaycast.gameObject.transform.position;
                ((RectTransform) transform).sizeDelta = new Vector2(70, 70);
                this.transform.parent = eventData.pointerCurrentRaycast.gameObject.transform;

                SetRaycastBlock(true);
                SetRaycastBlock(true, inventorySlotsParent);
            }
            else // outside the grid
            {
                var data = gameObject.GetComponent<UIItemDataHolder>().Data;
                ReleaseQuickbarIndexIfDoable(data);
                
                Destroy(gameObject);
                inventory.Items.Remove(data);
                SpawnItem(data);
                
                SetRaycastBlock(true);
                SetRaycastBlock(true, inventorySlotsParent);
            }
        }
        
        private void SpawnItem(ItemData data)
        {
            GameObject go = droppedItemsManager.GetNextBlock();

            Vector3 playerForward = playerMovementTracker.transform.forward;
            go.transform.position = playerMovementTracker.transform.position + playerForward;

            go.GetComponent<DroppedItemInformation>().FromBlock(new Block((BlockUV) data.ItemID), data.Amount);
            go.GetComponent<Rigidbody>().AddForce(playerForward, ForceMode.Impulse);
            
            droppedItemsManager.AddNewItem(go);
            droppedItemsManager.AddBoxColliderHandle(go.transform.GetChild(0).GetComponent<BoxCollider>());
        }
        
        private void SetRaycastBlock(bool state)
        {
            foreach (Transform child in transform.parent)
            {
                if (child.TryGetComponent(out CanvasGroup group))
                {
                    group.blocksRaycasts = state;
                }
            }
        }

        private void ReleaseQuickbarIndexIfDoable(ItemData data)
        {
            int prevQuickbarIndex = data.QuickbarIndex; 
                
            data.QuickbarIndex = -1;

            if (prevQuickbarIndex != -1)
            {
                quickBar[prevQuickbarIndex] = null;
                quickbarHitIndex = -1;
            }
        }

        private void SetRaycastBlock(bool state, Transform t)
        {
            foreach (Transform child in t)
            {
                if (child.TryGetComponent(out CanvasGroup group))
                {
                    group.blocksRaycasts = state;
                }
            }
        }
    }
}