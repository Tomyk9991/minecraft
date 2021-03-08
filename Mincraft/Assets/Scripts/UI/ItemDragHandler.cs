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

                gameObject.GetComponent<UIItemDataHolder>().Data.QuickbarIndex = -1;
            }
            else if (quickbarHitResult) // Quick bar
            {
                int quickbarHitIndex = eventData.pointerCurrentRaycast.gameObject.transform.GetSiblingIndex();

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
                this.transform.parent = eventData.pointerCurrentRaycast.gameObject.transform;

                SetRaycastBlock(true);
                SetRaycastBlock(true, inventorySlotsParent);
            }
            else // outside the grid
            {
                var data = gameObject.GetComponent<UIItemDataHolder>().Data;
                //Remove as UI Object
                Destroy(gameObject);
                //DropAmountDialog
                //Based on selected amount, remove from inventory
                inventory.Items.Remove(data);
                //Drop the actual object with selected amount
                SpawnItem(data);
                
                Debug.Log("outside the inventory");
            }
        }
        
        private void SpawnItem(ItemData data)
        {
            GameObject go = droppedItemsManager.GetNextBlock();

            Debug.Log(go.name);
            
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