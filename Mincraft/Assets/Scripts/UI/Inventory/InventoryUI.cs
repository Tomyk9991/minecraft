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
        [SerializeField] private Transform uiItemsParent = null;

        [Header("Positioning")] [SerializeField]
        private Vector2 intialGridPosition = Vector2.zero;

        [SerializeField] private Vector2 gridSize = Vector2.zero;

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

            inventory.OnRequestRedraw += OnRequestRedraw;
            inventory.OnItemAmountChanged += OnItemAmountChanged;
            inventory.OnNewItem += OnNewItem;
            inventory.OnSwapItems += OnSwapItems;
            inventory.OnItemMoved += OnItemMoved;
            inventory.OnItemDropped += OnItemDropped;
        }

        private void OnItemDropped(ItemChangedEventArgs args)
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

        private void OnItemMoved(ItemMovedEventArgs args) // not used
        {
        }

        private void OnSwapItems(ItemSwappedEventArgs args)
        {
            int oldX = args.OldItem.x;
            int oldY = args.OldItem.y;

            args.NewItem.CurrentGameObject.transform.localPosition = CalculatePosition(oldX, oldY);
        }

        private void OnNewItem(ItemChangedEventArgs args)
        {
            ItemData data = args.Item;
            if (!(data.Amount <= 0 || data.ItemID == (int) BlockUV.Air || data.ItemID == (int) BlockUV.None))
            {
                GameObject go = CreateItem(data.x, data.y, data.ItemID, data.Amount);
                data.CurrentGameObject = go;
                args.Items[data.x, data.y] = data;
            }
        }

        private void OnItemAmountChanged(ItemChangedEventArgs args)
        {
            args.Items[args.Item.x, args.Item.y].CurrentGameObject.transform.GetChild(0).GetComponent<TMP_Text>().text =
                args.Item.Amount.ToString();
        }

        public void OnRequestRedraw(InventoryRedrawEventArgs args)
        {
            if (logs)
                Debug.Log("Creating inventory");

            foreach (Transform child in uiItemsParent)
                Destroy(child.gameObject);

            for (int i = 0; i < args.Items.Length; i++)
            {
                ItemData data = args.Items[i];

                if (data != null)
                {
                    GameObject go = CreateItem(data.x, data.y, data.ItemID, data.Amount);
                    data.CurrentGameObject = go;

                    args.Items[data.x, data.y] = data;
                }
            }
        }

        private GameObject CreateItem(int x, int y, int id, int amount)
        {
            Vector3 position = CalculatePosition(x, y);

            GameObject go = Instantiate(uiItemPrefab, Vector3.zero, Quaternion.identity, uiItemsParent);
            go.GetComponent<RectTransform>().localPosition = position;

            Sprite itemSprite = ItemDictionary.GetValue((BlockUV) id);
            go.GetComponent<Image>().sprite = itemSprite;

            TMP_Text text = go.GetComponentInChildren<TMP_Text>();
            text.text = amount.ToString();

            return go;
        }

        private Vector3 CalculatePosition(int x, int y)
        {
            return new Vector3(
                gridSize.x * x + intialGridPosition.x,
                -gridSize.y * y + intialGridPosition.y,
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