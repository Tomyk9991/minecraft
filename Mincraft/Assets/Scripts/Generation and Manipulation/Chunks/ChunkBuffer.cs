using System;
using Core.Chunking.Threading;
using Core.Math;

namespace Core.Chunking
{
    public static class ChunkBuffer
    {
        private static ChunkColumn[] data;
        private static object mutex = new object();

        public static int dimension = 0;
        public static int DrawDistanceInChunks { get; private set; }
        public static int YBound { get; private set; }

        private static int minHeight;
        private static int maxHeight;

        public static void Init(int chunkSize, int _minHeight, int _maxHeight, int drawDistanceInChunks)
        {
            dimension = 2 * drawDistanceInChunks + 3;
            DrawDistanceInChunks = drawDistanceInChunks;
            minHeight = _minHeight;
            maxHeight = _maxHeight;

            YBound = (System.Math.Abs(minHeight) + System.Math.Abs(maxHeight)) / chunkSize;

            data = new ChunkColumn[dimension * dimension]; // Um eins in jede Richtung größer, als renderColumns
        }
        /// <summary>
        /// Shift all entries inside the array in the given direction. Only one direction supported
        /// </summary>
        /// <param name="direction">Desired direction you moved to</param>
        /// <exception cref="Exception">Throws an exception if enum is unidentified</exception>
        public static void Shift(Direction direction)
        {
            switch (direction)
            {
                // X value changed
                case Direction.Right:
                    {
                        MoveRight();
                        break;
                    }

                default: // value must be -1, so nothing changed
                    {
                        break;
                    }
            }
        }

        private static void MoveRight()
        {
            lock (mutex)
            {
                for (int x = 0; x < dimension - 1; x++)
                {
                    for (int y = 0; y < dimension; y++)
                    {
                        ChunkColumn from = data[Idx2D(x + 1, y)];
                        data[Idx2D(x, y)] = new ChunkColumn(from.GlobalPosition, new Int2(x, y), minHeight, maxHeight)
                        {
                            chunks = from.chunks
                        };

                        for (int h = 0; h < data[Idx2D(x, y)].chunks.Length; h++)
                        {
                            data[Idx2D(x, y)].chunks[h].LocalPosition = new Int3(x, h, y);
                        }

                        if (x > 0 && y > 0 && y < dimension - 1)
                            data[Idx2D(x, y)].DesiredForVisualization = true;
                    }
                }

                for (int y = 0; y < dimension; y++)
                {
                    for (int h = minHeight, localy = 0; h < maxHeight; h += 16, localy++)
                    {
                        data[Idx2D(0, y)][localy].ReleaseGameObject();
                    }
                }


                ChunkColumn leftNeighbour = data[Idx2D(dimension - 2, 0)];
                for (int y = 0; y < dimension; y++)
                {
                    Int2 globalPosition = new Int2(leftNeighbour.GlobalPosition.X + 16, leftNeighbour.GlobalPosition.Y + (y * 16));
                    Int2 localPosition = new Int2(dimension - 1, y);

                    ChunkColumn column = new ChunkColumn(globalPosition, localPosition, minHeight, maxHeight);

                    for (int h = minHeight, localy = 0; h < maxHeight; h += 16, localy++)
                    {
                        Chunk chunk = new Chunk()
                        {
                            LocalPosition = new Int3(dimension - 1, localy, y),
                            GlobalPosition = new Int3(globalPosition.X, y, globalPosition.Y)
                        };

                        column[localy] = chunk;
                    }

                    data[Idx2D(dimension - 1, y)] = column;

                    NoiseJob noiseJob = new NoiseJob()
                    {
                        Column = column
                    };
                    column.State = DrawingState.InNoiseQueue;

                    NoiseJobManager.NoiseJobManagerUpdaterInstance.AddJob(noiseJob);
                }
            }
        }

        public static Chunk GetChunk(Int3 local)
        {
            if (local.Y >= YBound || local.Y < 0)
                return null;

            lock (mutex)
            {
                return data[Idx2D(local.X, local.Z)][local.Y];
            }
        }

        public static ChunkColumn GetChunkColumn(int x, int z)
        {
            lock (mutex)
            {
                return data[Idx2D(x, z)];
            }
        }

        public static void SetChunkColumn(int x, int z, ChunkColumn value)
        {
            lock (mutex)
            {
                data[Idx2D(x, z)] = value;
            }
        }


        public static ChunkColumn[] Data
        {
            get
            {
                lock (mutex)
                {
                    return data;
                }
            }
        }

        private static int Idx2D(int x, int y)
            => ((2 * DrawDistanceInChunks + 3)) * x + y;
    }

    public enum Direction
    {
        None,
        Left, Right,
        Forward, Back,
    }
}