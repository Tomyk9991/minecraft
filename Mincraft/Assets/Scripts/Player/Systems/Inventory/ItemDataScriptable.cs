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
        [ArrayElementTitle("EnumType")] public List<ItemInformation> itemInformation;
    }

    [Serializable]
    public struct ItemInformation
    {
        public BlockUV EnumType;
        public Sprite Sprite;
    }
}
