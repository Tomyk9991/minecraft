using Core.Player.Systems.Inventory;
using UnityEngine;

namespace Core.Player.Interaction
{
    public class DroppedItemsCollector : MonoBehaviour
    {
        [SerializeField] private Transform followPlayerTarget = null;
        private DroppedItemsManager droppedItemsManager;
        private Inventory inventory;

        private void Start()
        {
            droppedItemsManager = DroppedItemsManager.Instance;
            inventory = Inventory.Instance;
        }
        
        private void Update()
        {
            transform.position = followPlayerTarget.transform.position;
        }

        //Called from Unity
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("InventoryItem"))
            {
                GameObject go = other.transform.parent.gameObject;
                DroppedItemInformation info = go.GetComponent<DroppedItemInformation>();
                
                // TODO Check if this kind of dropped item is still collectible (so check if the maxStackSize is exceeded)
                
                inventory.AddItemToInventory(info.ItemID, info.Amount);
                droppedItemsManager.AddToPool(go);
            }
        }
    }
}
