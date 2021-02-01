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
    public class InventoryUI : SingletonBehaviour<InventoryUI>, IConsoleToggle
    {
        [Header("References"), SerializeField] private Transform[] inventoryToggleTransforms = null;
        [SerializeField] private Transform uiInventoryItemsParent = null;

        [Header("Grid calculations"), SerializeField]
        private Vector2 marginLeftTop = Vector2.zero;

        [SerializeField] private Vector2 gridSize = Vector2.zero;


        [Space] [SerializeField] private Inventory inventory = null;
        [SerializeField] private GameObject uiItemPrefab = null;

        private IFullScreenUIToggle[] disableOnInventoryAppear = null;
        private bool showingInventory = false;

        private DroppedItemsManager droppedItemsManager;
        private PlayerMovementTracker playerMovementTracker;


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

            InitializeInventoryUI();
        }

        private void InitializeInventoryUI()
        {
            for (int i = 0; i < inventory.Items.Count; i++)
            {
                ItemData item = inventory.Items[i];

                if (item != null)
                {
                    GameObject go = CreateItemInventory(item);
                }
            }
        }

        private GameObject CreateItemInventory(ItemData item)
        {
            GameObject go = Instantiate(uiItemPrefab, Vector3.zero, Quaternion.identity, uiInventoryItemsParent);

            Sprite itemSprite = ItemDictionary.GetValue((BlockUV) item.ItemID);
            go.GetComponent<Image>().sprite = itemSprite;

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