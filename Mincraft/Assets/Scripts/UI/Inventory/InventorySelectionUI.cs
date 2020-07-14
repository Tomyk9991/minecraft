using Core.UI.Console;
using UnityEngine;

namespace Core.UI.Ingame
{
    public class InventorySelectionUI : MonoBehaviour, IConsoleToggle
    {
        [SerializeField] private RectTransform[] slots;
        [SerializeField] private RectTransform selectedSlotItem;

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

        private void Update()
        {
            float scrollDirection = Input.mouseScrollDelta.y;

            if (scrollDirection != 0)
            {
                slotIndex += scrollDirection > 0 ? +1 : -1;
                slotIndex %= slots.Length;

                slotIndex = slotIndex < 0 ? slots.Length - 1 : slotIndex;
                selectedSlotItem.position = slots[slotIndex].position;
            }
        }
    }
}
