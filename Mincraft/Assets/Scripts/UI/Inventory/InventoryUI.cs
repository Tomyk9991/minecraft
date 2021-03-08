using System.Linq;
using Core.Builder;
using Core.Managers;
using Core.Player.Interaction;
using Core.Player.Systems.Inventory;
using Core.Saving;
using Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.Ingame
{
    public class InventoryUI : SingletonBehaviour<InventoryUI>, IConsoleToggle
    {
        [Header("References")] 
        [SerializeField] private Transform[] inventoryToggleTransforms = null;
        [SerializeField] private Transform uiInventoryItemsParent = null;
        [SerializeField] private Transform[] quickbarItems = null;
        
        [Space] 
        [SerializeField] private Inventory inventory = null;
        [SerializeField] private GameObject uiItemPrefab = null;

        private IFullScreenUIToggle[] disableOnInventoryAppear = null;
        private bool showingInventory = false;


        public bool Enabled
        {
            get => this.enabled;
            set => this.enabled = value;
        }

        private void Start()
        {
            disableOnInventoryAppear = FindObjectsOfType<MonoBehaviour>().OfType<IFullScreenUIToggle>().ToArray();
            InitializeInventoryUI();
            InitializeQuickbarUI();
        }

        public void ItemAmountChanged(ItemData data)
        {
            data.CurrentGameObject.GetComponentInChildren<TMP_Text>().text = data.Amount.ToString();
        }

        public void ItemCreated(ItemData data)
        {
            GameObject go = CreateItemInventory(data, uiInventoryItemsParent);
            data.CurrentGameObject = go;
        }

        private void InitializeQuickbarUI()
        {
            for (int i = 0; i < inventory.QuickBar.Items.Length; i++)
            {
                if (inventory.QuickBar[i] != null)
                {
                    GameObject go = CreateItemInventory(inventory.QuickBar[i], quickbarItems[i]);
                    go.transform.position = quickbarItems[i].position;
                    inventory.QuickBar[i].CurrentGameObject = go;
                }
            }
        }

        //Called from unity
        public void OnItemSearchValueChanged(string newValue)
        {
            if (newValue == "")
            {
                foreach (var data in inventory.Items)
                {
                    data.CurrentGameObject.SetActive(true);
                }
            }

            foreach (var data in inventory.Items)
            {
                if (data.QuickbarIndex != -1)
                {
                    data.CurrentGameObject.SetActive(true);
                }
                else
                {
                    string name = ItemDictionary.GetName((BlockUV) data.ItemID);
                    
                    string manipulatedName = name.Replace(" ", "").ToLower();
                    string manipulatedNewValue = newValue.Replace(" ", "").ToLower();
                    
                    data.CurrentGameObject.SetActive(manipulatedName.Contains(manipulatedNewValue));
                }
            }
        }

        private void InitializeInventoryUI()
        {
            for (int i = 0; i < inventory.Items.Count; i++)
            {
                ItemData item = inventory.Items[i];

                if (item != null && item.QuickbarIndex == -1)
                {
                    GameObject go = CreateItemInventory(item, uiInventoryItemsParent);
                    item.CurrentGameObject = go;
                }
            }
        }

        private GameObject CreateItemInventory(ItemData item, Transform parent)
        {
            GameObject go = Instantiate(uiItemPrefab, Vector3.zero, Quaternion.identity, parent);

            Sprite itemSprite = ItemDictionary.GetValue((BlockUV) item.ItemID);
            go.GetComponent<Image>().sprite = itemSprite;
            go.GetComponent<UIItemDataHolder>().Data = item;

            TMP_Text text = go.GetComponentInChildren<TMP_Text>();
            text.text = item.Amount.ToString();

            return go;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
                SetInventoryWorkability(!showingInventory);
        }

        private void SetInventoryWorkability(bool state)
        {
            showingInventory = state;
            CursorVisibilityManager.Instance.ToggleMouseVisibility(showingInventory);

            foreach (IFullScreenUIToggle toggleObject in disableOnInventoryAppear)
            {
                toggleObject.Enabled = !showingInventory;
            }

            foreach (Transform child in inventoryToggleTransforms)
            {
                child.gameObject.SetActive(showingInventory);
            }
        }
    }
}