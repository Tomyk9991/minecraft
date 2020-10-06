using System.Linq;
using Core.Managers;
using Core.Player.Systems;
using UnityEngine;
using Utilities;

namespace Core.UI.Ingame
{
    public class InventoryUI : MonoBehaviour, IConsoleToggle
    {
        [SerializeField] private int gridSizeRows = 8;
        [SerializeField] private int gridSizeColumns = 7;

        [SerializeField] private Transform[] inventoryToggleTransforms = null;
        [SerializeField] private Transform slotParent = null;

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
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
                ToggleInventory();
        }

        private void ToggleInventory()
        {
            showingInventory = !showingInventory;
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