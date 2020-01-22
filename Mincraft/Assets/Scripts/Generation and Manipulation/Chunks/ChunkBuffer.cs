using System;
using Core.Chunking.Threading;
using Core.Math;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Core.Chunking
{
    public static class ChunkBuffer
    {
        private static ChunkColumn[] data;
        private static object mutex = new object();
        private static NoiseJobManager noiseJobManager;

        public static int dimension = 0;
        public static int DrawDistanceInChunks { get; private set; }
        public static int YBound { get; private set; }

        private static int minHeight;
        private static int maxHeight;

        public static void Init(int chunkSize, int _minHeight, int _maxHeight, int drawDistanceInChunks)
        {
            noiseJobManager = NoiseJobManager.NoiseJobManagerUpdaterInstance;
            
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
                case Direction.Right:
                    MoveRight();
                    break;
                case Direction.Left:
                    MoveLeft();
                    break;
                case Direction.Forward:
                    MoveUp();
                    break;
                case Direction.Back:
                    MoveDown();
                    break;
            }
        }

        private static void MoveUp()
        {
            lock (mutex)
            {
                //Shift everything
                for (int x = 0; x < dimension; x++)
                {
                    for (int y = 0; y < dimension - 1; y++)
                    {
                        ChunkColumn column = data[Idx2D(x, y)];
                        column = data[Idx2D(x, y + 1)];
                        column.LocalPosition = new Int2(x, y);

                        for (int h = 0; h < column.chunks.Length; h++)
                        {
                            column[h].LocalPosition = new Int3(x, h, y);
                        }
                        
                        column.DesiredForVisualization = x != 0 && 
                                                         y != 0 && 
                                                         x != dimension - 1 &&
                                                         y != dimension - 1;
                        
                        data[Idx2D(x, y)] = column;
                    } 
                }

                //Create new
                ChunkColumn rightNeighbour = data[Idx2D(0, dimension - 2)];
                for (int x = 0; x < dimension; x++)
                {
                    Int2 globalPosition = new Int2(rightNeighbour.GlobalPosition.X + (x * 16), rightNeighbour.GlobalPosition.Y + 16);
                    Int2 localPosition = new Int2(x, dimension - 1);

                    ChunkColumn column = new ChunkColumn(globalPosition, localPosition, minHeight, maxHeight);
                    for (int h = minHeight, localy = 0; h < maxHeight; h += 16, localy++)
                    {
                        Chunk chunk = new Chunk()
                        {
                            LocalPosition = new Int3(x, localy, dimension - 1),
                            GlobalPosition = new Int3(globalPosition.X, h, globalPosition.Y)
                        };

                        column[localy] = chunk;
                    }

                    data[Idx2D(x, dimension - 1)] = column;
                    
                    NoiseJob noiseJob = new NoiseJob()
                    {
                        Column = column
                    };
                    column.State = DrawingState.InNoiseQueue;

                    noiseJobManager.Add(noiseJob);
                }

                for (int x = 0; x < dimension; x++)
                {
                    for (int h = minHeight, localy = 0; h < maxHeight; h += 16, localy++)
                    {
                        data[Idx2D(x, 0)][localy].ReleaseGameObject();
                    }
                    
                    data[Idx2D(x, 0)].State = DrawingState.NoiseReady;
                }
            }
        }

        private static void MoveDown()
        {
            lock (mutex)
            {
                //Shift everything
                for (int x = 0; x < dimension; x++)
                {
                    for (int y = dimension - 1; y >= 1; y--)
                    {
                        ChunkColumn column = data[Idx2D(x, y)];
                        column = data[Idx2D(x, y - 1)];
                        column.LocalPosition = new Int2(x, y);

                        for (int h = 0; h < column.chunks.Length; h++)
                        {
                            column[h].LocalPosition = new Int3(x, h, y);
                        }
                        
                        column.DesiredForVisualization = x != 0 && 
                                                         y != 0 && 
                                                         x != dimension - 1 &&
                                                         y != dimension - 1;
                        
                        data[Idx2D(x, y)] = column;
                    } 
                }

                //Create new
                ChunkColumn rightNeighbour = data[Idx2D(0, 1)];
                for (int x = 0; x < dimension; x++)
                {
                    Int2 globalPosition = new Int2(rightNeighbour.GlobalPosition.X + (x * 16), rightNeighbour.GlobalPosition.Y - 16);
                    Int2 localPosition = new Int2(x, 0);

                    ChunkColumn column = new ChunkColumn(globalPosition, localPosition, minHeight, maxHeight);
                    for (int h = minHeight, localy = 0; h < maxHeight; h += 16, localy++)
                    {
                        Chunk chunk = new Chunk()
                        {
                            LocalPosition = new Int3(x, localy, 0),
                            GlobalPosition = new Int3(globalPosition.X, h, globalPosition.Y)
                        };

                        column[localy] = chunk;
                    }

                    data[Idx2D(x, 0)] = column;
                    
                    NoiseJob noiseJob = new NoiseJob()
                    {
                        Column = column
                    };
                    column.State = DrawingState.InNoiseQueue;

                    noiseJobManager.Add(noiseJob);
                }

                for (int x = 0; x < dimension; x++)
                {
                    for (int h = minHeight, localy = 0; h < maxHeight; h += 16, localy++)
                    {
                        data[Idx2D(x, dimension - 1)][localy].ReleaseGameObject();
                    }
                    
                    data[Idx2D(x, dimension - 1)].State = DrawingState.NoiseReady;
                }
            }
        }

        private static void MoveLeft()
        {
            lock (mutex)
            {
                //Shift everything
                for (int x = dimension - 1; x >= 1; x--)
                {
                    for (int y = 0; y < dimension; y++)
                    {
                        ChunkColumn column = data[Idx2D(x, y)];
                        column = data[Idx2D(x - 1, y)];
                        column.LocalPosition = new Int2(x, y);

                        for (int h = 0; h < column.chunks.Length; h++)
                        {
                            column[h].LocalPosition = new Int3(x, h, y);
                        }
                        
                        column.DesiredForVisualization = x != 0 && 
                                                         y != 0 && 
                                                         x != dimension - 1 &&
                                                         y != dimension - 1;
                        
                        data[Idx2D(x, y)] = column;
                    } 
                }

                //Create new
                ChunkColumn rightNeighbour = data[Idx2D(1, 0)];
                for (int y = 0; y < dimension; y++)
                {
                    Int2 globalPosition = new Int2(rightNeighbour.GlobalPosition.X - 16, rightNeighbour.GlobalPosition.Y + (y * 16));
                    Int2 localPosition = new Int2(0, y);

                    ChunkColumn column = new ChunkColumn(globalPosition, localPosition, minHeight, maxHeight);
                    for (int h = minHeight, localy = 0; h < maxHeight; h += 16, localy++)
                    {
                        Chunk chunk = new Chunk()
                        {
                            LocalPosition = new Int3(0, localy, y),
                            GlobalPosition = new Int3(globalPosition.X, h, globalPosition.Y)
                        };

                        column[localy] = chunk;
                    }

                    data[Idx2D(0, y)] = column;
                    
                    NoiseJob noiseJob = new NoiseJob()
                    {
                        Column = column
                    };
                    column.State = DrawingState.InNoiseQueue;

                    noiseJobManager.Add(noiseJob);
                }

                for (int y = 0; y < dimension; y++)
                {
                    for (int h = minHeight, localy = 0; h < maxHeight; h += 16, localy++)
                    {
                        data[Idx2D(dimension - 1, y)][localy].ReleaseGameObject();
                    }
                    
                    data[Idx2D(dimension - 1, y)].State = DrawingState.NoiseReady;
                }
            }
        }

        private static void MoveRight()
        {
            lock (mutex)
            {
                //Shift everything
                for (int x = 0; x < dimension - 1; x++)
                {
                    for (int y = 0; y < dimension; y++)
                    {
                        ChunkColumn column = data[Idx2D(x, y)];
                        column = data[Idx2D(x + 1, y)];
                        column.LocalPosition = new Int2(x, y);

                        for (int h = 0; h < column.chunks.Length; h++)
                        {
                            column[h].LocalPosition = new Int3(x, h, y);
                        }
                        
                        column.DesiredForVisualization = x != 0 && 
                                                         y != 0 && 
                                                         x != dimension - 1 &&
                                                         y != dimension - 1;
                        
                        data[Idx2D(x, y)] = column;
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
                            GlobalPosition = new Int3(globalPosition.X, h, globalPosition.Y)
                        };

                        column[localy] = chunk;
                    }

                    data[Idx2D(dimension - 1, y)] = column;

                    NoiseJob noiseJob = new NoiseJob()
                    {
                        Column = column
                    };
                    column.State = DrawingState.InNoiseQueue;

                    noiseJobManager.Add(noiseJob);
                }

                for (int y = 0; y < dimension; y++)
                {
                    for (int h = minHeight, localy = 0; h < maxHeight; h += 16, localy++)
                    {
                        data[Idx2D(0, y)][localy].ReleaseGameObject();
                    }

                    data[Idx2D(0, y)].State = DrawingState.NoiseReady;
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
        
        public static Chunk GetChunk(int x, int y, int z)
        {
            if (y >= YBound || y < 0)
                return null;

            lock (mutex)
            {
                return data[Idx2D(x, z)][y];
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