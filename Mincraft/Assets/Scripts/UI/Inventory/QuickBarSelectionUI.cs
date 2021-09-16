using System;
using Core.Builder;
using Core.Player.Systems.Inventory;
using Core.Saving;
using Extensions;
using UnityEngine;

namespace Core.UI.Ingame
{
    public class QuickBarSelectionUI : SingletonBehaviour<QuickBarSelectionUI>, IConsoleToggle
    {
        public int SelectedIndex => slotIndex;
        public event Action<BlockUV> OnSelectionChanged;

        [SerializeField] private RectTransform[] slots = null;
        [SerializeField] private RectTransform selectedSlotItem = null;
        [SerializeField] private bool directionRight = true;
        
        
        private int slotIndex = 0;
        private Inventory inventory;
        
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
            this.inventory = Inventory.Instance;
            if (ResourceIO.LoadCached<Inventory>(new InventoryFileIdentifier(), out OutputContext context))
            {
                int selectedIndex = ((InventorySavingManager.InventoryLoadingContext<ItemData, int>) context).additionalData;
                
                this.slotIndex = selectedIndex;
            }
            
            //TODO make it possible without invoke
            Invoke(nameof(SetSelected), 0.05f);
        }

        void SetSelected()
        {
            SetSelectedSlotPosition(this.slotIndex);
        }
        
        private void Update()
        {
            if (Input.anyKeyDown)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0))
                {
                    slotIndex = 9;
                }
                
                if(Input.GetKeyDown(KeyCode.Alpha1)) { slotIndex = 0; SetSelectedSlotPosition(slotIndex); }
                if(Input.GetKeyDown(KeyCode.Alpha2)) { slotIndex = 1; SetSelectedSlotPosition(slotIndex); }
                if(Input.GetKeyDown(KeyCode.Alpha3)) { slotIndex = 2; SetSelectedSlotPosition(slotIndex); }
                if(Input.GetKeyDown(KeyCode.Alpha4)) { slotIndex = 3; SetSelectedSlotPosition(slotIndex); }
                if(Input.GetKeyDown(KeyCode.Alpha5)) { slotIndex = 4; SetSelectedSlotPosition(slotIndex); }
                if(Input.GetKeyDown(KeyCode.Alpha6)) { slotIndex = 5; SetSelectedSlotPosition(slotIndex); }
                if(Input.GetKeyDown(KeyCode.Alpha7)) { slotIndex = 6; SetSelectedSlotPosition(slotIndex); }
                if(Input.GetKeyDown(KeyCode.Alpha8)) { slotIndex = 7; SetSelectedSlotPosition(slotIndex); }
                if(Input.GetKeyDown(KeyCode.Alpha9)) { slotIndex = 8; SetSelectedSlotPosition(slotIndex); }
            }
            
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

            ItemData data = inventory.QuickBar[slotIndex];
            OnSelectionChanged?.Invoke(data == null ? BlockUV.Air : (BlockUV) data.ItemID);
        }
    }
}
