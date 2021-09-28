using System;
using Core.Chunks;
using UnityEngine;

namespace Player.Interaction.ItemWorldAdder
{
    public interface IItemWorldAdder
    {
        Vector2Int ItemRange { get; }
        void Initialize(ScriptableObject initializer);
        void OnPlace(int itemID, ChunkReferenceHolder holder, Ray ray, RaycastHit hit);
    }
}