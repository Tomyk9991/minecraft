using UnityEngine;

namespace Core.Builder.Generation
{
    [System.Serializable]
    public class Biom
    {
        public string Name;

        [Header("Min- and Max noise values")]
        public float minValue;
        public float maxValue;

        [Header("Top layer")]
        public BlockUV topLayerBlock;
        public int topLayerBaseHeight = 1;
        public int topLayerNoiseHeight = 2;
        public float topLayerNoise = 0.4f;

        [Header("Mid layer")]
        public BlockUV midLayerBlock;
        public int midBaseHeight = -24;
        public int midBaseNoiseHeight = 2;
        public float midBaseNoise = 0.05f;

        [Header("Low layer")]
        public BlockUV lowerLayerBlock;
        public int lowerBaseHeight = -24;
        public int lowerBasNoisHeight = 4;
        public float lowerBaseNoise = 0.05f;

        [Header("Mountain layer")]
        public int lowerMountainHeight = 48;
        public int lowerMinHeight = -12;
        public float lowerMountainFrequency = 0.008f;

        [Header("Cave layer")]
        public float caveFrequency = 2.5f;
        public int caveSize = 30;

        [Header("Tree layer")]
        public BlockUV treeTrunkBlock;
        public BlockUV treeLeafBlock;
        [Range(0f, 1f)] public float treeProbability = 0.11f;
        public float treeZoomLevel = 1.08f;
    }
}
