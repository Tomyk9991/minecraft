using System.Collections.Generic;
using System.Linq;
using Core.Builder;
using Core.Player.Interaction;
using Core.Player.Interaction.ItemWorldAdder;
using Core.UI;
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
        
        private int handIndex = 0;
        
        
        public bool Enabled
        {
            get => this.enabled;
            set => this.enabled = value;
        }
        
        public static readonly BlockUV[] CircuitBlocks = new BlockUV[]
        {
            BlockUV.AndGate,
            BlockUV.OrGate,
            BlockUV.NotGate,
        };
        
        private List<Circuit> circuits = new List<Circuit>();
        
        private void Start()
        {
            BlockAdder.OnAddBlock += OnAddBlock;
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
    }
}