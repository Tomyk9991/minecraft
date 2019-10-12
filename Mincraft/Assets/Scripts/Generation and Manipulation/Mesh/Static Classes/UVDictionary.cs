using System;
using UnityEngine;
using UnityInspector.PropertyAttributes;

namespace Core.Builder
{
    public class UVDictionary : MonoBehaviour
    {
        [ArrayElementTitle("EnumType")]
        [SerializeField] private BlockInformation[] data = null;

        private static readonly UVData[] notFoundData = 
        {
            new UVData(7f / 16f, 1f / 16f, 1f / 16f, 1f / 16f),
            new UVData(7f / 16f, 1f / 16f, 1f / 16f, 1f / 16f),
            new UVData(7f / 16f, 1f / 16f, 1f / 16f, 1f / 16f),
            new UVData(7f / 16f, 1f / 16f, 1f / 16f, 1f / 16f),
            new UVData(7f / 16f, 1f / 16f, 1f / 16f, 1f / 16f),
            new UVData(7f / 16f, 1f / 16f, 1f / 16f, 1f / 16f),
        };

        private static UVData[][] dictionary;

        private static bool[] isSolidInformation;
        private static bool[] isTransparentInformation;
        private static float[] meshOffsetInformation;
        private static float[] transparancyLevel;

        private void Awake()
        {
            dictionary = new UVData[data.Length][];
            isSolidInformation = new bool[data.Length];
            isTransparentInformation = new bool[data.Length];
            meshOffsetInformation = new float[data.Length];
            transparancyLevel = new float[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                dictionary[(int)data[i].EnumType] = new []
                {
                    new UVData(data[i].Forward.TileX / 16f, data[i].Forward.TileY / 16f, data[i].Forward.SizeX / 16f, data[i].Forward.SizeY / 16f),
                    new UVData(data[i].Back.TileX / 16f, data[i].Back.TileY / 16f, data[i].Back.SizeX / 16f, data[i].Back.SizeY / 16f),
                    new UVData(data[i].Up.TileX / 16f, data[i].Up.TileY / 16f, data[i].Up.SizeX / 16f, data[i].Up.SizeY / 16f),

                    new UVData(data[i].Down.TileX / 16f, data[i].Down.TileY / 16f, data[i].Down.SizeX / 16f, data[i].Down.SizeY / 16f),
                    new UVData(data[i].Left.TileX / 16f, data[i].Left.TileY / 16f, data[i].Left.SizeX / 16f, data[i].Left.SizeY / 16f),
                    new UVData(data[i].Right.TileX / 16f, data[i].Right.TileY / 16f, data[i].Right.SizeX / 16f, data[i].Right.SizeY / 16f),
                };

                isSolidInformation[(int) data[i].EnumType] = data[i].isSolid;
                isTransparentInformation[(int) data[i].EnumType] = data[i].isTransparent;
                meshOffsetInformation[(int)data[i].EnumType] = data[i].Meshoffset;
                transparancyLevel[(int)data[i].EnumType] = data[i].transparencyLevel;
            }
        }

        public static float TransparencyLevelID(BlockUV id)
        {
            if (id < 0 || (int)id > transparancyLevel.Length - 1)
                return 0f;
            return transparancyLevel[(int)id];
        }

        public static void Clear()
        {
            dictionary = null;
        }

        public static UVData[] GetValue(BlockUV id)
        {
            if (id < 0 || (int) id > dictionary.Length - 1)
                return notFoundData;

            return dictionary[(int) id];
        }

        public static float MeshOffsetID(BlockUV id)
        {
            if (id < 0 || (int)id > meshOffsetInformation.Length - 1)
                return 0f;
            return meshOffsetInformation[(int)id];
        }

        public static bool IsSolidID(BlockUV id)
        {
            if (id < 0 || (int)id > isSolidInformation.Length - 1)
                return false;

            return isSolidInformation[(int) id];
        }

        public static bool IsTransparentID(BlockUV id)
        {
            if (id < 0 || (int)id > isTransparentInformation.Length - 1)
                return false;

            return isTransparentInformation[(int) id];
        }
    }

    [Serializable]
    public struct BlockInformation
    {
        public BlockUV EnumType;
        public UVData Forward;
        public UVData Back;
        public UVData Up;
        public UVData Down;
        public UVData Left;
        public UVData Right;

        public float Meshoffset;
        public float transparencyLevel;

        public bool isSolid; // Bestimmt, ob es für den Collider relevant ist
        public bool isTransparent; // Bestimmt, ob es eine Transparent durch den Block gibt
    }
}