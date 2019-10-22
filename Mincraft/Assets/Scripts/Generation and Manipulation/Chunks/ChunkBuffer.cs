using Core.Math;

namespace Core.Chunking
{
    public static class ChunkBuffer
    {
        private static ChunkColumn[] columns;
        private static object mutex = new object();

        public static int DrawDistanceInChunks { get; private set; }
        public static int YBound { get; private set; }

        public static void Init(int chunkSize, int minHeight, int maxHeight, int drawDistanceInChunks)
        {
            DrawDistanceInChunks = drawDistanceInChunks;

            YBound = (System.Math.Abs(minHeight) + System.Math.Abs(maxHeight)) / chunkSize;

            columns = new ChunkColumn[(2 * drawDistanceInChunks + 3) * (2 * drawDistanceInChunks + 3)]; // Um eins in jede Richtung größer, als renderColumns
        }

        public static Chunk GetChunk(Int3 local)
        {
            if (local.Y >= YBound || local.Y < 0)
                return null;

            lock (mutex)
            {
                return columns[GetFlattenIndex2DNoise(local.X, local.Z)][local.Y];
            }
        }

        public static ChunkColumn GetColumn(int x, int z)
        {
            lock (mutex)
            {
                return columns[GetFlattenIndex2DNoise(x, z)];
            }
        }

        public static ChunkColumn[] Columns
        {
            get
            {
                lock (mutex)
                {
                    return columns;
                }
            }
        }

        public static void SetColumn(int x, int y, ChunkColumn column)
        {
            lock (mutex)
            {
                ChunkBuffer.columns[GetFlattenIndex2DNoise(x, y)] = column;
            }
        }

        private static int GetFlattenIndex2DNoise(int x, int y)
            => ((2 * DrawDistanceInChunks + 3)) * x + y;
    }
}