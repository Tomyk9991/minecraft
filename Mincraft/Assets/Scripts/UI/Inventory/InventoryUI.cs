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
using Debug = UnityEngine.Debug;

namespace Core.UI.Ingame
{
    public class InventoryUI : SingletonBehaviour<InventoryUI>, IConsoleToggle
    {
        [Header("References")] [SerializeField]
        private Transform[] inventoryToggleTransforms = null;

        [SerializeField] private Inventory inventory = null;
        [SerializeField] private GameObject uiItemPrefab = null;
        [SerializeField] private Transform uiInventoryItemsParent = null;
        [SerializeField] private Transform uiQuickbarItemsParent = null;
        

        [Header("Inventory positioning")] 
        [SerializeField] private Vector2 initialGridPosition = Vector2.zero;
        [SerializeField] private Vector2 gridSize = Vector2.zero;

        [Header("Quickbar positioning")] 
        [SerializeField] private Vector2 initialQuickbarPosition = Vector2.zero;
        [SerializeField] private Vector2 quickBarItemSize = Vector2.zero;

        
        [Header("Debugging")] [SerializeField] private bool logs = false;


        private IFullScreenUIToggle[] disableOnInventoryAppear = null;
        private bool showingInventory = false;
        
        private DroppedItemsManager droppedItemsManager;
        private PlayerMovementTracker playerMovementTracker;

        public bool Logs => logs;

        public bool Enabled
        {
            get => this.enabled;
            set => this.enabled = value;
        }

        private void Start()
        {
            disableOnInventoryAppear = FindObjectsOfType<MonoBehaviour>().OfType<IFullScreenUIToggle>().ToArray();
            droppedItemsManager = DroppedItemsManager.Instance;
            playerMovementTracker = PlayerMovementTracker.Instance;

            Debug.Log("Inventory null?: " + inventory == null);

            inventory.OnRequestRedraw += OnInventoryRequestRedraw;
            inventory.OnItemAmountChanged += OnItemInventoryAmountChanged;
            inventory.OnNewItem += OnInventoryNewItem;
            inventory.OnSwapItems += OnInventorySwapItems;
            inventory.OnItemMoved += OnInventoryItemMoved;
            inventory.OnItemDropped += OnInventoryItemDropped;

            inventory.InitQuickbar();
            
            inventory.QuickBar.OnRequestRedraw += OnQuickbarRequestRedraw;
            inventory.QuickBar.OnItemAmountChanged += OnQuickbarAmountChanged;
            inventory.QuickBar.OnSwapItems += OnQuickbarSwapItems;
            inventory.QuickBar.OnItemMoved += OnQuickbarItemMoved;
            inventory.QuickBar.OnItemDropped += OnQuickbarItemDropped;

            inventory.QuickBar.RaiseOnRequestRedraw();
        }
        
        private void OnQuickbarRequestRedraw(QuickbarRedrawEventArgs args)
        {
            if (logs)
                Debug.Log("Creating Quickbar");

            foreach (Transform child in uiQuickbarItemsParent)
                Destroy(child.gameObject);
            
            for (int i = 0; i < args.Items.Length; i++)
            {
                ItemData data = args.Items[i];

                if (data != null)
                {
                    GameObject go = CreateItemQuickbar(data.quickbarIndex, data.ItemID, data.Amount);
                    data.CurrentGameObject = go;

                    args.Items[data.quickbarIndex] = data;
                }
            }
            
        }

        private void OnQuickbarItemDropped(QuickbarItemChangedEventArgs obj)
        {
            
        }

        private void OnQuickbarItemMoved(QuickbarItemMovedEventArgs obj)
        {
            
        }

        private void OnQuickbarSwapItems(QuickbarItemSwappedEventArgs obj)
        {
            
        }

        private void OnQuickbarAmountChanged(QuickbarItemChangedEventArgs obj)
        {
            
        }

        #region Inventory Events

        private void OnInventoryItemDropped(InventoryItemChangedEventArgs args)
        {
            ItemData item = args.Item;
            
            for (int i = 0; i < item.Amount; i++)
            {
                GameObject go = droppedItemsManager.GetNextBlock();
                go.transform.position =
                    playerMovementTracker.transform.position + playerMovementTracker.transform.forward;

                go.GetComponent<DroppedItemInformation>().FromBlock(new Block((BlockUV) item.ItemID));
                go.GetComponent<Rigidbody>().AddForce(playerMovementTracker.transform.forward, ForceMode.Impulse);

                droppedItemsManager.AddNewItem(go);
                droppedItemsManager.AddBoxColliderHandle(go.transform.GetChild(0).GetComponent<BoxCollider>());
            }
        }

        private void OnInventoryItemMoved(InventoryItemMovedEventArgs args) // not used
        {
        }

        private void OnInventorySwapItems(InventoryItemSwappedEventArgs args)
        {
            int oldX = args.OldItem.x;
            int oldY = args.OldItem.y;

            args.NewItem.CurrentGameObject.transform.localPosition = CalculatePositionInventoryGrid(oldX, oldY);
        }

        private void OnInventoryNewItem(InventoryItemChangedEventArgs args)
        {
            ItemData data = args.Item;
            if (!(data.Amount <= 0 || data.ItemID == (int) BlockUV.Air || data.ItemID == (int) BlockUV.None))
            {
                GameObject go = CreateItemInventory(data.x, data.y, data.ItemID, data.Amount);
                data.CurrentGameObject = go;
                args.Items[data.x, data.y] = data;
            }
        }

        private void OnItemInventoryAmountChanged(InventoryItemChangedEventArgs args)
        {
            args.Items[args.Item.x, args.Item.y].CurrentGameObject.transform.GetChild(0).GetComponent<TMP_Text>().text =
                args.Item.Amount.ToString();
        }

        public void OnInventoryRequestRedraw(InventoryRedrawEventArgs args)
        {
            if (logs)
                Debug.Log("Creating inventory");

            foreach (Transform child in uiInventoryItemsParent)
                Destroy(child.gameObject);

            for (int i = 0; i < args.Items.Length; i++)
            {
                ItemData data = args.Items[i];

                if (data != null)
                {
                    GameObject go = CreateItemInventory(data.x, data.y, data.ItemID, data.Amount);
                    data.CurrentGameObject = go;

                    args.Items[data.x, data.y] = data;
                }
            }
        }
        

        #endregion

        #region Quickbar Events
        

        #endregion

        private GameObject CreateItemQuickbar(int index, int id, int amount)
        {
            Vector3 position = CalculatePositionQuickbar(index);

            GameObject go = Instantiate(uiItemPrefab, Vector2.zero, Quaternion.identity, uiQuickbarItemsParent);
            go.GetComponent<RectTransform>().localPosition = position;

            Sprite itemSprite = ItemDictionary.GetValue((BlockUV) id);
            go.GetComponent<Image>().sprite = itemSprite;

            TMP_Text text = go.GetComponentInChildren<TMP_Text>();
            text.text = amount.ToString();

            return go;
        }
        
        private GameObject CreateItemInventory(int x, int y, int id, int amount)
        {
            Vector3 position = CalculatePositionInventoryGrid(x, y);

            GameObject go = Instantiate(uiItemPrefab, Vector3.zero, Quaternion.identity, uiInventoryItemsParent);
            go.GetComponent<RectTransform>().localPosition = position;

            Sprite itemSprite = ItemDictionary.GetValue((BlockUV) id);
            go.GetComponent<Image>().sprite = itemSprite;

            TMP_Text text = go.GetComponentInChildren<TMP_Text>();
            text.text = amount.ToString();

            return go;
        }

        private Vector3 CalculatePositionInventoryGrid(int x, int y)
        {
            return new Vector3(
                gridSize.x * x + initialGridPosition.x,
                -gridSize.y * y + initialGridPosition.y,
                0f);
        }

        private Vector3 CalculatePositionQuickbar(int index)
        {
            return new Vector3(quickBarItemSize.x * index + initialQuickbarPosition.x,
                initialQuickbarPosition.y,
                0f);
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