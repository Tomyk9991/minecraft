using Core.UI.Console;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.Ingame
{
    public class QuickBarSelectionConsole : MonoBehaviour, IConsoleToggle
    {
        [SerializeField] private RectTransform[] slots;
        [SerializeField] private RectTransform selectedSlotItem;
        [SerializeField] private Image image = null;
        

        private int slotIndex = 0;

        public bool Enabled
        {
            get => this.enabled;
            set
            {
                foreach (var slot in slots)
                    slot.gameObject.SetActive(value);
                
                selectedSlotItem.gameObject.SetActive(value);
                image.enabled = value;
                this.enabled = value;
            }
        }

        private void Update()
        {
            float scrollDirection = Input.mouseScrollDelta.y;

            if (scrollDirection != 0)
            {
                slotIndex += System.Math.Sign(scrollDirection);
                slotIndex %= slots.Length;

                slotIndex = slotIndex < 0 ? slots.Length - 1 : slotIndex;
                selectedSlotItem.position = slots[slotIndex].position;
            }
        }
    }
}
