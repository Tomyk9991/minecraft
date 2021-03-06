using Core.Player.Systems.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    public class ContentSizeFitterToggler : MonoBehaviour
    {
        [SerializeField] private Inventory _inventory = null;
        private ContentSizeFitter fitter;
        
        private void Start()
        {
            this.fitter = GetComponent<ContentSizeFitter>();

            _inventory.OnCriticalSizeExceeded += length =>
            {
                if (length == 1)
                {
                    Canvas.ForceUpdateCanvases();
                    fitter.verticalFit = ContentSizeFitter.FitMode.MinSize;
                }
            };
        }
    }
}