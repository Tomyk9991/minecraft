using System;
using UnityEngine;
using Utilities;

namespace Core.Builder
{
    public class UVDictionary : MonoBehaviour
    {
        [SerializeField] private UVDataScriptable scriptable = null;
        
        private static readonly UVData[] notFoundData = 
        {
            new UVData(7f / 16f, 1f / 16f, 1f / 16f, 1f / 16f),
            new UVData(7f / 16f, 1f / 16f, 1f / 16f, 1f / 16f),
            new UVData(7f / 16f, 1f / 16f, 1f / 16f, 1f / 16f),
            new UVData(7f / 16f, 1f / 16f, 1f / 16f, 1f / 16f),
            new UVData(7f / 16f, 1f / 16f, 1f / 16f, 1f / 16f),
            new UVData(7f / 16f, 1f / 16f, 1f / 16f, 1f / 16f),
        };

        private static Array2D<UVData[]> uvDataInformation;
        
        private static bool[] isSolidInformation;
        private static bool[] isTransparentInformation;
        private static bool[] canFaceDifferentDirectionsInformation;
        private static RenderingTechnique[] renderingTechnique;
        private static float[] meshOffsetInformation;
        private static float[] transparancyLevel;

        private void Awake()
        {
            var data = scriptable.blockInformation;
            uvDataInformation = new Array2D<UVData[]>(data.Count);
            
            isSolidInformation = new bool[data.Count];
            isTransparentInformation = new bool[data.Count];
            canFaceDifferentDirectionsInformation = new bool[data.Count];
            renderingTechnique = new RenderingTechnique[data.Count];
            
            meshOffsetInformation = new float[data.Count];
            transparancyLevel = new float[data.Count];

            for (int i = 0; i < data.Count; i++)
            {
                uvDataInformation[(int)data[i].EnumType] = new []
                {
                    new UVData(data[i].Forward.TileX / 16f, data[i].Forward.TileY / 16f, data[i].Forward.SizeX / 16f, data[i].Forward.SizeY / 16f),
                    new UVData(data[i].Back.TileX / 16f, data[i].Back.TileY / 16f, data[i].Back.SizeX / 16f, data[i].Back.SizeY / 16f),
                    new UVData(data[i].Up.TileX / 16f, data[i].Up.TileY / 16f, data[i].Up.SizeX / 16f, data[i].Up.SizeY / 16f),

                    new UVData(data[i].Down.TileX / 16f, data[i].Down.TileY / 16f, data[i].Down.SizeX / 16f, data[i].Down.SizeY / 16f),
                    new UVData(data[i].Left.TileX / 16f, data[i].Left.TileY / 16f, data[i].Left.SizeX / 16f, data[i].Left.SizeY / 16f),
                    new UVData(data[i].Right.TileX / 16f, data[i].Right.TileY / 16f, data[i].Right.SizeX / 16f, data[i].Right.SizeY / 16f),
                };

                canFaceDifferentDirectionsInformation[(int) data[i].EnumType] = data[i].CanFaceDifferentDirections;
                isSolidInformation[(int) data[i].EnumType] = data[i].isSolid;
                isTransparentInformation[(int) data[i].EnumType] = data[i].isTransparent;
                renderingTechnique[(int) data[i].EnumType] = data[i].RenderingTechnique;
                
                meshOffsetInformation[(int)data[i].EnumType] = data[i].Meshoffset;
                transparancyLevel[(int)data[i].EnumType] = data[i].transparencyLevel;
            }
        }

        public static float TransparencyLevelID(in BlockUV id)
        {
            if (id < 0 || (int)id > transparancyLevel.Length - 1)
                return 0f;
            return transparancyLevel[(int)id];
        }

        public static void Clear()
        {
            uvDataInformation = null;
        }

        public static RenderingTechnique RenderingTechnique(in BlockUV id)
        {
            if (id < 0 || (int) id > renderingTechnique.Length - 1)
            {
                Debug.LogWarning($"Rendering technique information not found: {(int) id}");
                return Builder.RenderingTechnique.Block;
            }

            return renderingTechnique[(int) id];
        }

        public static bool CanFaceInDifferentDirections(in BlockUV id)
        {
            if (id < 0 || (int) id > canFaceDifferentDirectionsInformation.Length - 1)
            {
                Debug.LogWarning("Rotation information not found");
                return false;
            }

            return canFaceDifferentDirectionsInformation[(int) id];
        }

        public static UVData[] GetValue(in BlockUV id)
        {
            if (id < 0 || (int) id > uvDataInformation.Length - 1)
                return notFoundData;

            return uvDataInformation[(int) id];
        }

        public static float MeshOffsetID(in BlockUV id)
        {
            if (id < 0 || (int)id > meshOffsetInformation.Length - 1)
                return 0f;
            return meshOffsetInformation[(int)id];
        }

        public static bool IsSolidID(in BlockUV id)
        {
            if (id < 0 || (int)id > isSolidInformation.Length - 1)
                return false;

            return isSolidInformation[(int) id];
        }

        public static bool IsTransparentID(in BlockUV id)
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
        public bool CanFaceDifferentDirections; //Bestimmt  ob Objekt drehbar ist oder nicht (Ofen)
        public RenderingTechnique RenderingTechnique;
    }

    public enum RenderingTechnique
    {
        Block = 0,
        Sprite3D = 1,
        CustomMesh = 2
    }
}