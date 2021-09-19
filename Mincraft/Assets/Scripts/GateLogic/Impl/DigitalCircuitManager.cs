using System.Collections.Generic;
using Core.Builder;
using Core.Player.Interaction;
using Core.UI;
using Extensions;
using UnityEngine;
using Utilities;

namespace GateLogic.Impl
{
    public class DigitalCircuitManager : MonoBehaviour, IConsoleToggle, IFullScreenUIToggle
    {
        [SerializeField] private GameObject lineRendererPrefab = null;
        [SerializeField] private Camera cameraRef = null;
        [SerializeField] private Transform handPosition = null;
        

        [SerializeField] private float RayDistance = 7.0f;
        [SerializeField] private LayerMask hitMask;
        private CenterMouseRaycaster raycaster;
        
        private LineRenderer currentLineRenderer;
        private int handIndex = 0;
        
        
        public bool Enabled
        {
            get => this.enabled;
            set => this.enabled = value;
        }
        
        public static readonly List<BlockUV> CircuitBlocks = new List<BlockUV>
        {
            BlockUV.AndGate,
            BlockUV.OrGate,
            BlockUV.NotGate,
        };
        
        private List<Circuit> circuits = new List<Circuit>();
        
        private void Start()
        {
            AddBlock.OnAddBlock += OnAddBlock;
            RemoveBlock.OnRemoveBlock += OnRemoveBlock;
            raycaster = new CenterMouseRaycaster(cameraRef, RayDistance, hitMask);
        }

        private void OnRemoveBlock(BlockUV block)
        {
            if (!CircuitBlocks.Contains(block)) return;
        }

        private void OnAddBlock(BlockUV block)
        {
            if (!CircuitBlocks.Contains(block)) return;
            Debug.Log("Add digital block");
        }


        
        //TODO make sure, it works with the current inventory system. ATM only blocks are supported
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {

                if (raycaster.Raycast())
                {
                    if (!this.currentLineRenderer)
                    {
                        GameObject go = Instantiate(lineRendererPrefab);
                        this.currentLineRenderer = go.GetComponent<LineRenderer>();
                    }
                    
                    if (currentLineRenderer.positionCount == 0)
                    {
                        currentLineRenderer.AddPoint(raycaster.RayCastHit.point);
                        handIndex = currentLineRenderer.AddPoint(handPosition.position);
                    }
                    else
                    {
                        currentLineRenderer.InsertPoint(currentLineRenderer.positionCount - 1,
                            raycaster.RayCastHit.point);
                        handIndex++;
                    }
                }
            }

            if (this.currentLineRenderer)
            {
                currentLineRenderer.SetPosition(handIndex, handPosition.position);
            }
        }
    }
}