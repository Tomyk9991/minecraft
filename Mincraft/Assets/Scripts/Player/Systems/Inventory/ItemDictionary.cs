﻿using Core.Builder;
using UnityEngine;
using Utilities;

namespace Core.Player.Interaction
{
    public class ItemDictionary : MonoBehaviour
    {
        [SerializeField] private ItemDataScriptable scriptable = null;
        private static Array2D<Sprite> dictionary;

        private void Awake()
        {
            var data = scriptable.itemInformation;

            dictionary = new Array2D<Sprite>(data.Count);

            for (int i = 0; i < data.Count; i++)
            {
                dictionary[(int) data[i].EnumType] = data[i].Sprite;
            }
        }

        public static Sprite GetValue(BlockUV itemId)
        {
            return dictionary[(int) itemId];
        }
    }
}