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


        private static Transform inventorySlotsParent = null;

        private void Start()
        {
            if (droppedItemsManager == null) droppedItemsManager = DroppedItemsManager.Instance;
            if (playerMovementTracker == null) playerMovementTracker = PlayerMovementTracker.Instance;
            if (inventory == null) inventory = Inventory.Instance;
            if (quickBar == null) quickBar = inventory.QuickBar;

            if(root == null) root = GameObject.Find("UI").transform;
            if (inventorySlotsParent == null)
                inventorySlotsParent = Resources.FindObjectsOfTypeAll<GameObject>()
                    .FirstOrDefault(g => g.name == "Inventory Slots")
                    .transform; //GameObject.Find("Inventory Slots").transform;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
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

            ItemData data = inventoryGridHitResult || quickbarHitResult
                ? gameObject.GetComponent<UIItemDataHolder>().Data : null;


            if (inventoryGridHitResult)
            {
                transform.parent = inventorySlotsParent;
                SetRaycastBlock(true);

                data.QuickbarIndex = -1;
            }
            else if (quickbarHitResult) // Quick bar
            {
                int quickbarHitIndex = eventData.pointerCurrentRaycast.gameObject.transform.GetSiblingIndex();

                if (quickBar[quickbarHitIndex] != null)
                {
                    //Return back to inventory
                    transform.parent = inventorySlotsParent;
                    SetRaycastBlock(true);
                    return;
                }
                else
                {
                    quickBar[quickbarHitIndex] = data;
                    data.QuickbarIndex = quickbarHitIndex;
                    
                    Debug.Log("item ID: added to quickbar" + (BlockUV) data.ItemID);
                }

                // quickbar
                this.transform.position = eventData.pointerCurrentRaycast.gameObject.transform.position;
                this.transform.parent = eventData.pointerCurrentRaycast.gameObject.transform;

                SetRaycastBlock(true);
                SetRaycastBlock(true, inventorySlotsParent);
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
                if (child.TryGetComponent(out CanvasGroup group))
                {
                    group.blocksRaycasts = state;
                }
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