using System;
using Core.Chunks;
using UnityEngine;

namespace Player.Interaction.ItemWorldAdder
{
    public interface IItemWorldAdder
    {
        Vector2 ItemRange { get; }
        void Initialize();
        void OnPlace(int itemID, ChunkReferenceHolder holder, Ray ray, RaycastHit hit);
    }
}