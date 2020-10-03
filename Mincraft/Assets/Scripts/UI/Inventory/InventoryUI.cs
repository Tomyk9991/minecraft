using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Managers;
using Core.UI.Console;
using UnityEngine;

namespace Core.UI.Ingame
{
    public class InventoryUI : MonoBehaviour, IConsoleToggle
    {
        [SerializeField] private Transform[] inventoryToggleTransforms = null;
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

        //Called from Unity
        public void OnButtonClick(int buttonIndex)
        {
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