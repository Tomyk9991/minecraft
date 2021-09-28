using System;
using Core.Chunks;
using Core.Managers;
using Core.UI;
using Core.UI.Ingame;
using Player.Interaction.ItemWorldAdder;
using UnityEngine;
using Utilities;

namespace Core.Player.Interaction.ItemWorldAdder
{
    public class Adder : MonoBehaviour, IMouseUsable, IConsoleToggle, IFullScreenUIToggle
    {
        public static event Action OnPlace;
        
        public float DesiredTimeUntilAction
        {
            get => timeBetweenRemove;
            set => timeBetweenRemove = value;
        }

        public float RaycastDistance
        {
            get => raycastHitable;
            set => raycastHitable = value;
        }

        public int MouseButtonIndex
        {
            get => mouseButtonIndex;
            set => mouseButtonIndex = value;
        }

        public bool Enabled
        {
            get => this.enabled;
            set => this.enabled = value;
        }
        
        [Header("Add behaviours")] 
        private IItemWorldAdder[] adders =
        {
            new BlockAdder()
        };

        [Header("References")] 
        [SerializeField]
        private Camera cameraRef = null;

        [Space] 
        [SerializeField] private int mouseButtonIndex = 1;
        [SerializeField] private float raycastHitable = 1000f;
        [SerializeField] private float timeBetweenRemove = 0.1f;
        [SerializeField] private LayerMask hitMask = 0;
        [SerializeField] private int itemID = -1;
        private GameManager gameManager;

        private RaycastHit hit;
        private readonly Vector3 centerScreenNormalized = new Vector3(0.5f, 0.5f, 0f);
        
        private QuickBarSelectionUI currentSelectionUI;

        private Timer timer;

        private void Start()
        {
            timer = new Timer(DesiredTimeUntilAction);
            
            currentSelectionUI = QuickBarSelectionUI.Instance;
            currentSelectionUI.OnSelectionChanged += SetItem;


            for (int i = 0; i < this.adders.Length; i++)
                this.adders[i].Initialize();
        }

        private void OnValidate()
            => timer.HardReset(timeBetweenRemove);

        private void SetItem(int itemID)
        {
            this.itemID = itemID;
        }

        private void Update()
        {
            if (Input.GetMouseButton(mouseButtonIndex) && timer.TimeElapsed(Time.deltaTime))
            {
                DoRaycast();
            }

            if (Input.GetMouseButtonDown(mouseButtonIndex))
            {
                DoRaycast();
                timer.Reset();
            }
        }

        private void DoRaycast()
        {
            Ray ray = cameraRef.ViewportPointToRay(centerScreenNormalized);
            
            if (Physics.Raycast(ray, out hit, RaycastDistance, hitMask))
            {
                OnPlace?.Invoke();
                
                ChunkReferenceHolder holder;
                if (!hit.transform.TryGetComponent(out holder))
                    return;

                IItemWorldAdder adder = null;

                for (int i = 0; i < this.adders.Length; i++)
                {
                    if (this.adders[i].ItemRange.x <= itemID && itemID <= this.adders[i].ItemRange.y)
                    {
                        adder = this.adders[i];
                        break;
                    }
                }

                if (adder != null)
                {
                    adder.OnPlace(itemID, holder, ray, hit);
                }
                else
                {
                    Debug.Log("Adder not found");
                }
            }
        }
    }
}