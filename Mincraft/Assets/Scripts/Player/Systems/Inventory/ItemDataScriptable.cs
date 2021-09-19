using System;
using System.Collections.Generic;
using Attributes;
using Core.Builder;
using UnityEngine;

namespace Core.Player.Interaction
{
    [CreateAssetMenu(fileName = "ItemDataScriptable", menuName = "Scriptable Objects/ItemData")]
    public class ItemDataScriptable : ScriptableObject
    {
        [ArrayElementTitle("Name")] public List<ItemInformation> itemInformation;
    }

    [Serializable]
    public struct ItemInformation
    {
        public bool IsBlock;
        [DrawIfTrue(nameof(IsBlock))] public BlockUV BlockID;
        [DrawIfFalse(nameof(IsBlock))] public int ItemID;
        public Sprite Sprite;
        public string Name;
    }
}
