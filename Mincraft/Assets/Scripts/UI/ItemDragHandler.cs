using System.Linq;
using Core.Player;
using Core.Player.Interaction;
using Core.Player.Systems.Inventory;
using Core.Saving;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.UI
{
    public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler
    {
        private Transform root = null;

        private static DroppedItemsManager droppedItemsManager;
        private static PlayerMovementTracker playerMovementTracker;
        private static Inventory inventory = null;
        private static QuickBar quickBar = null;


        private Transform inventorySlotsParent = null;

        private void Start()
        {
            if (droppedItemsManager == null) droppedItemsManager = DroppedItemsManager.Instance;
            if (playerMovementTracker == null) playerMovementTracker = PlayerMovementTracker.Instance;
            if (inventory == null) inventory = Inventory.Instance;
            if (quickBar == null) quickBar = inventory.QuickBar;

            root = GameObject.Find("UI").transform;
            inventorySlotsParent = GameObject.Find("Inventory Slots").transform;
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


            if (inventoryGridHitResult)
            {
                transform.parent = inventorySlotsParent;
                SetRaycastBlock(true);
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
                    Debug.Log("Slot was empty");
                    
                    //Get reference for the moving item, either from inventory.Items or quickbar[...]
                    //ItemData item = inventory.Items.
                    //quickBar[quickbarHitIndex] = inventory.Items.First(item => item.CurrentGameObject == eventData.pointerCurrentRaycast.gameObject);

                    Debug.Log("quick bar got added an item");
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