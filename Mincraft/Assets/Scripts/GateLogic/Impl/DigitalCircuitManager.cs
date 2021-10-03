using System.Collections.Generic;
using System.Linq;
using Core.Builder;
using Core.Player.Interaction;
using Core.Player.Interaction.ItemWorldAdder;
using Core.UI;
using UnityEngine;

namespace GateLogic.Impl
{
    public class DigitalCircuitManager : MonoBehaviour, IConsoleToggle, IFullScreenUIToggle
    {
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
        }
        
        public static bool IsCircuitBlock(BlockUV block)
            => CircuitBlocks.Contains(block);

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