using System.Linq;
using Core.Builder;
using Core.Managers;
using Core.Math;
using Core.Player.Interaction;
using Core.Player.Systems.Inventory;
using Core.Saving;
using Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Core.UI.Ingame
{
    public class InventoryUI : SingletonBehaviour<InventoryUI>, IConsoleToggle
    {
        [Header("References")] [SerializeField]
        private Transform[] inventoryToggleTransforms = null;

        [SerializeField] private Inventory inventory = null;
        [SerializeField] private GameObject uiItemPrefab = null;
        [SerializeField] private Transform uiItemsParent = null;

        [Header("Debugging")] [SerializeField] private bool debugging = false;


        private IFullScreenUIToggle[] disableOnInventoryAppear = null;
        private bool showingInventory = false;

        private Array2D<GameObject> items;

        public bool Enabled
        {
            get => this.enabled;
            set => this.enabled = value;
        }

        private void Start()
        {
            disableOnInventoryAppear = FindObjectsOfType<MonoBehaviour>().OfType<IFullScreenUIToggle>().ToArray();
            inventory.OnInventoryChanged += OnInventoryChanged;
            items = new Array2D<GameObject>(inventory.Width, inventory.Height);
        }

        private void OnInventoryChanged(ItemChangedEventArgs args)
        {
            if (debugging)
            {
                string kindOfRequest = "";
                if (args.RequestRedraw) kindOfRequest += "Request new ";
                if (args.ItemSlotModified) kindOfRequest += "Itemslot modified ";
                if (args.ItemSlotRequest) kindOfRequest += "ItemSlot requested ";

                Debug.Log(kindOfRequest + args.Item);
            }

            if (args.RequestRedraw)
            {
                RedrawInventoryCompletely(args);
                return;
            }

            if (args.ItemSlotRequest)
            {
                CreateItem(args.Item.x, args.Item.y, args.Item.ItemID, args.Item.Amount);
            }

            if (args.ItemSlotModified)
            {
                items[args.Item.x, args.Item.y].GetComponentInChildren<TMP_Text>().text = args.Item.Amount.ToString();
            }
        }

        private void CreateItem(int x, int y, int id, int amount)
        {
            Vector3 position = new Vector3((40.0f * x) - 120.0f, (40.0f * y) + 140.0f, 0f);

            GameObject go = Instantiate(uiItemPrefab, Vector3.zero, Quaternion.identity, uiItemsParent);
            go.GetComponent<RectTransform>().localPosition = position;

            Sprite itemSprite = ItemDictionary.GetValue((BlockUV) id);
            go.GetComponent<Image>().sprite = itemSprite;

            TMP_Text text = go.GetComponentInChildren<TMP_Text>();
            text.text = amount.ToString();

            items[x, y] = go;
        }

        private void RedrawInventoryCompletely(ItemChangedEventArgs args)
        {
            foreach (Transform child in uiItemsParent)
                Destroy(child.gameObject);

            for (int i = 0; i < args.Items.Length; i++)
            {
                ItemData data = args.Items[i];
                
                if (data.ItemID == 0 || data.ItemID == (int) BlockUV.None || data.Amount == 0)
                    continue;
                
                CreateItem(data.x, data.y, data.ItemID, data.Amount);
            }
        }

        public Int2 IndexFromGameObject(GameObject go)
        {
            for (int y = 0; y < items.Height; y++)
            {
                for (int x = 0; x < items.Width; x++)
                {
                    if (items[x, y] == go)
                    {
                        return new Int2(x, y);
                    }
                }
            }

            return new Int2(-1, -1);
        }

        public bool SlotAvailable(int x, int y)
        {
            return items[x, y] == null;
        }

        public void Swap(int oldX, int oldY, int newX, int newY, Vector3 oldDragStartPosition)
        {
            if (debugging)
                Debug.Log("Swap");
            //Change in UI-Stage
            Vector3 tempPos = items[newX, newY].transform.localPosition;

            GameObject temp = items[newX, newY];
            items[newX, newY] = items[oldX, oldY];
            items[oldX, oldY] = temp;

            items[newX, newY].transform.localPosition = tempPos;
            items[oldX, oldY].transform.localPosition = oldDragStartPosition;

            //Change in data-Stage
            inventory.Swap(oldX, oldY, newX, newY);
        }

        public void MoveGameObjectFromTo(int oldX, int oldY, int newX, int newY)
        {
            if (debugging)
                Debug.Log("Moved gameObject from: (" + oldX + " | " + oldY + ") to (" + newX + " | " + newY + ")");
            //Change in UI-Stage
            items[newX, newY] = items[oldX, oldY];
            items[oldX, oldY] = null;

            //Change in data-Stage
            inventory.MoveItemFromTo(oldX, oldY, newX, newY);
        }
        
        public void Delete(int x, int y)
        {
            if (debugging)
                Debug.Log("Remove gameObject at: (" + x + " | " + y +  ")");
            
            Destroy(items[x, y]);
            items[x, y] = null;

            inventory.Remove(x, y);
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