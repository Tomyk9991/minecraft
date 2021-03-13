using System;
using Core.Player.Systems.Inventory;
using Core.Saving;
using Extensions;
using UnityEngine;

namespace Core.UI.Ingame
{
    public class QuickBarSelectionUI : SingletonBehaviour<QuickBarSelectionUI>, IConsoleToggle
    {
        public int SelectedIndex => slotIndex;
        
        [SerializeField] private RectTransform[] slots = null;
        [SerializeField] private RectTransform selectedSlotItem = null;
        [SerializeField] private bool directionRight = true;
        private int slotIndex = 0;
        
        public bool Enabled
        {
            get => this.enabled;
            set
            {
                foreach (var slot in slots)
                    slot.gameObject.SetActive(value);
                
                selectedSlotItem.gameObject.SetActive(value);
                this.enabled = value;
            }
        }

        private void Start()
        {
            if (ResourceIO.LoadCached<Inventory>(new InventoryFileIdentifier(), out OutputContext context))
            {
                int selectedIndex = ((PlayerSavingManager.Wrapper<ItemData, int>) context).additionalData;
                
                this.slotIndex = selectedIndex;
                SetSelectedSlotPosition(this.slotIndex);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                slotIndex = 9;
            }
            if(Input.GetKeyDown(KeyCode.Alpha1)) slotIndex = 0;
            if(Input.GetKeyDown(KeyCode.Alpha2)) slotIndex = 1;
            if(Input.GetKeyDown(KeyCode.Alpha3)) slotIndex = 2;
            if(Input.GetKeyDown(KeyCode.Alpha4)) slotIndex = 3;
            if(Input.GetKeyDown(KeyCode.Alpha5)) slotIndex = 4;
            if(Input.GetKeyDown(KeyCode.Alpha6)) slotIndex = 5;
            if(Input.GetKeyDown(KeyCode.Alpha7)) slotIndex = 6;
            if(Input.GetKeyDown(KeyCode.Alpha8)) slotIndex = 7;
            if(Input.GetKeyDown(KeyCode.Alpha9)) slotIndex = 8;
            
            SetSelectedSlotPosition(slotIndex);
            
            float scrollDirection = Input.mouseScrollDelta.y;

            if (scrollDirection != 0)
            {
                slotIndex += directionRight ? System.Math.Sign(scrollDirection) : -System.Math.Sign(scrollDirection);
                slotIndex %= 10;

                slotIndex = slotIndex < 0 ? 9 : slotIndex;
                SetSelectedSlotPosition(slotIndex);
            }
        }

        private void SetSelectedSlotPosition(int pos)
        {
            selectedSlotItem.position = slots[pos].position;
        }
    }
}
