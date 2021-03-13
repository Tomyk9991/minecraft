using System.Linq;
using Core.Builder;
using Core.Managers;
using Core.Player;
using Core.Player.Interaction;
using Core.Player.Systems.Inventory;
using Core.Saving;
using Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.Ingame
{
    public class SplitManagerUI : SingletonBehaviour<SplitManagerUI>, IConsoleToggle
    {
        public bool Enabled
        {
            get => this.enabled;
            set => this.enabled = value;
        }
     
        [Header("References")] 
        [SerializeField] private Transform[] splitManagerToggleTransforms = null;
        
        [Header("Split references")] 
        [SerializeField] private TMP_Text itemName = null;
        [SerializeField] private TMP_Text splitRatio = null;
        [SerializeField] private Button acceptButton = null;
        [SerializeField] private Button closeButton = null;
        [SerializeField] private Slider slider = null;
        
        private IFullScreenUIToggle[] disableOnInventoryAppear = null;
        private bool showingSplitManager = false;
        
        private ItemData data;
        private Transform targetObject = null;
        private Transform latestParent = null;
        
        private DroppedItemsManager droppedItemsManager;
        private PlayerMovementTracker playerMovementTracker;
        private Inventory inventory;
        
        private void Start()
        {
            droppedItemsManager = DroppedItemsManager.Instance;
            playerMovementTracker = PlayerMovementTracker.Instance;
            inventory = Inventory.Instance;

            disableOnInventoryAppear = FindObjectsOfType<MonoBehaviour>().OfType<IFullScreenUIToggle>().ToArray();
        }

        public void OnAcceptButtonClicked()
        {
            int splitAmount = (int) slider.value;

            if (splitAmount == data.Amount) // All items got selected, remove em all
            {
                Destroy(targetObject.transform);
                inventory.Items.Remove(data);
            }
            else
            {
                //Object must be in inventory, so you can just add negative amount to decrease the amount of items in the inventory
                inventory.AddBlockToInventory(new Block((BlockUV) data.ItemID), -splitAmount);
                targetObject.parent = latestParent;
                targetObject.position = latestParent.position;
            }
            
            SpawnItem(data, splitAmount);
            
            SetSplitManagerWorkability(false);
        }

        public void OnCloseButtonClicked()
        {
            targetObject.parent = latestParent;
            targetObject.position = latestParent.position;
            SetSplitManagerWorkability(false);
        }

        public void OnSliderValueChanged(float val)
        {
            splitRatio.SetText((int) slider.value + " / " + data.Amount);
        }
        
        public void Split(ItemData data, Transform targetObject, Transform latestParent)
        {
            this.targetObject = targetObject;
            this.latestParent = latestParent;
            
            SetSplitManagerWorkability(!showingSplitManager);
            SetItemDataInformation(data);
        }

        private void SetItemDataInformation(ItemData data)
        {
            this.data = data;
            itemName.SetText(((BlockUV) data.ItemID).ToString());
            
            slider.minValue = 1;
            slider.maxValue = data.Amount;
            
            splitRatio.SetText((int) slider.value + " / " + data.Amount);
        }

        private void SetSplitManagerWorkability(bool state)
        {
            showingSplitManager = state;
            CursorVisibilityManager.Instance.ToggleMouseVisibility(showingSplitManager);

            foreach (IFullScreenUIToggle toggleObject in disableOnInventoryAppear)
            {
                toggleObject.Enabled = !showingSplitManager;
            }

            foreach (Transform child in splitManagerToggleTransforms)
            {
                child.gameObject.SetActive(showingSplitManager);
            }
        }
        
        private void SpawnItem(ItemData data, int amount)
        {
            GameObject go = droppedItemsManager.GetNextBlock();

            Vector3 playerForward = playerMovementTracker.transform.forward;
            go.transform.position = playerMovementTracker.transform.position + playerForward;

            go.GetComponent<DroppedItemInformation>().FromBlock(new Block((BlockUV) data.ItemID), amount);
            go.GetComponent<Rigidbody>().AddForce(playerForward, ForceMode.Impulse);
            
            droppedItemsManager.AddNewItem(go);
            droppedItemsManager.AddBoxColliderHandle(go.transform.GetChild(0).GetComponent<BoxCollider>());
        }
    }
}
