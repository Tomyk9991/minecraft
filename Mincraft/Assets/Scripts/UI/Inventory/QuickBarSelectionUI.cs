using Core.Saving;
using Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.Ingame
{
    public class QuickBarSelectionUI : SingletonBehaviour<QuickBarSelectionUI>, IConsoleToggle, IFullScreenUIToggle
    {
        [SerializeField] private RectTransform[] slots = null;
        [SerializeField] private RectTransform selectedSlotItem = null;
        [SerializeField] private Image image = null;
        
        private int slotIndex = 0;
        private GameObject[] items = new GameObject[10];

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

        public bool SlotAvailable(int index)
        {
            return items[index] == null;
        }

        public void AddToBar(int index, GameObject go)
        {
            items[index] = go;

            RectTransform tr = go.GetComponent<RectTransform>();
            tr.sizeDelta = new Vector3(30, 30);
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
