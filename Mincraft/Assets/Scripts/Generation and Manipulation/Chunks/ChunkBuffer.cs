using System;
using Core.Builder;
using Core.Chunking.Threading;
using Core.Math;
using Core.Player;

namespace Core.Chunking
{
    public static class ChunkBuffer
    {
        public static bool UsingChunkBuffer { get; set; } = true;
        
        private static ChunkColumn[] data;
        private static object globalMutex = new object();
        private static JobManager jobManager;
        
        public static int Dimension { get; private set; }
        public static int DrawDistanceInChunks { get; private set; }
        public static int YBound { get; private set; }

        private static int minHeight;
        private static int maxHeight;
        private static int chunkSize = 0x10;
        
        public static void Init(int chunkSize, int _minHeight, int _maxHeight, int drawDistanceInChunks)
        {
            jobManager = JobManager.JobManagerUpdaterInstance;
            //noiseJobManager = NoiseJobManager.NoiseJobManagerUpdaterInstance;

            Dimension = 2 * drawDistanceInChunks + 3;
            DrawDistanceInChunks = drawDistanceInChunks;
            minHeight = _minHeight;
            maxHeight = _maxHeight;

            YBound = (System.Math.Abs(minHeight) + System.Math.Abs(maxHeight)) / chunkSize;

            data = new ChunkColumn[Dimension * Dimension]; // Um eins in jede Richtung größer, als renderColumns
        }
        #region Move
        
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
            lock (globalMutex)
            {
                //Shift everything
                for (int x = 0; x < Dimension; x++)
                {
                    for (int y = 0; y < Dimension - 1; y++)
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
                                                         x != Dimension - 1 &&
                                                         y != Dimension - 1;
                        
                        data[Idx2D(x, y)] = column;
                    } 
                }

                //Create new
                ChunkColumn rightNeighbour = data[Idx2D(0, Dimension - 2)];
                for (int x = 0; x < Dimension; x++)
                {
                    Int2 globalPosition = new Int2(rightNeighbour.GlobalPosition.X + (x * 16), rightNeighbour.GlobalPosition.Y + 16);
                    Int2 localPosition = new Int2(x, Dimension - 1);

                    ChunkColumn column = new ChunkColumn(globalPosition, localPosition, minHeight, maxHeight);
                    for (int h = minHeight, localy = 0; h < maxHeight; h += 16, localy++)
                    {
                        Chunk chunk = new Chunk()
                        {
                            LocalPosition = new Int3(x, localy, Dimension - 1),
                            GlobalPosition = new Int3(globalPosition.X, h, globalPosition.Y),
                            
                            ChunkColumn =  column
                        };

                        column[localy] = chunk;
                    }

                    data[Idx2D(x, Dimension - 1)] = column;
                    
                    
                    NoiseJob noiseJob = new NoiseJob()
                    {
                        Column = column
                    };

                    jobManager.Add(noiseJob);
                }

                for (int x = 0; x < Dimension; x++)
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
            lock (globalMutex)
            {
                //Shift everything
                for (int x = 0; x < Dimension; x++)
                {
                    for (int y = Dimension - 1; y >= 1; y--)
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
                                                         x != Dimension - 1 &&
                                                         y != Dimension - 1;
                        
                        data[Idx2D(x, y)] = column;
                    } 
                }

                //Create new
                ChunkColumn rightNeighbour = data[Idx2D(0, 1)];
                for (int x = 0; x < Dimension; x++)
                {
                    Int2 globalPosition = new Int2(rightNeighbour.GlobalPosition.X + (x * 16), rightNeighbour.GlobalPosition.Y - 16);
                    Int2 localPosition = new Int2(x, 0);

                    ChunkColumn column = new ChunkColumn(globalPosition, localPosition, minHeight, maxHeight);
                    for (int h = minHeight, localy = 0; h < maxHeight; h += 16, localy++)
                    {
                        Chunk chunk = new Chunk()
                        {
                            LocalPosition = new Int3(x, localy, 0),
                            GlobalPosition = new Int3(globalPosition.X, h, globalPosition.Y),
                            
                            ChunkColumn =  column
                        };

                        column[localy] = chunk;
                    }

                    data[Idx2D(x, 0)] = column;

                    NoiseJob noiseJob = new NoiseJob()
                    {
                        Column = column
                    };
                    jobManager.Add(noiseJob);
                }

                for (int x = 0; x < Dimension; x++)
                {
                    for (int h = minHeight, localy = 0; h < maxHeight; h += 16, localy++)
                    {
                        data[Idx2D(x, Dimension - 1)][localy].ReleaseGameObject();
                    }
                    
                    data[Idx2D(x, Dimension - 1)].State = DrawingState.NoiseReady;
                }
            }
        }

        private static void MoveLeft()
        {
            lock (globalMutex)
            {
                //Shift everything
                for (int x = Dimension - 1; x >= 1; x--)
                {
                    for (int y = 0; y < Dimension; y++)
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
                                                         x != Dimension - 1 &&
                                                         y != Dimension - 1;
                        
                        data[Idx2D(x, y)] = column;
                    } 
                }

                //Create new
                ChunkColumn rightNeighbour = data[Idx2D(1, 0)];
                for (int y = 0; y < Dimension; y++)
                {
                    Int2 globalPosition = new Int2(rightNeighbour.GlobalPosition.X - 16, rightNeighbour.GlobalPosition.Y + (y * 16));
                    Int2 localPosition = new Int2(0, y);

                    ChunkColumn column = new ChunkColumn(globalPosition, localPosition, minHeight, maxHeight);
                    for (int h = minHeight, localy = 0; h < maxHeight; h += 16, localy++)
                    {
                        Chunk chunk = new Chunk()
                        {
                            LocalPosition = new Int3(0, localy, y),
                            GlobalPosition = new Int3(globalPosition.X, h, globalPosition.Y),
                            
                            ChunkColumn =  column
                        };

                        column[localy] = chunk;
                    }

                    data[Idx2D(0, y)] = column;

                    NoiseJob noiseJob = new NoiseJob()
                    {
                        Column = column
                    };
                    jobManager.Add(noiseJob);
                }

                for (int y = 0; y < Dimension; y++)
                {
                    for (int h = minHeight, localy = 0; h < maxHeight; h += 16, localy++)
                    {
                        data[Idx2D(Dimension - 1, y)][localy].ReleaseGameObject();
                    }
                    
                    data[Idx2D(Dimension - 1, y)].State = DrawingState.NoiseReady;
                }
            }
        }

        private static void MoveRight()
        {
            lock (globalMutex)
            {
                //Shift everything
                for (int x = 0; x < Dimension - 1; x++)
                {
                    for (int y = 0; y < Dimension; y++)
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
                                                         x != Dimension - 1 &&
                                                         y != Dimension - 1;
                        
                        data[Idx2D(x, y)] = column;
                    }
                }

                ChunkColumn leftNeighbour = data[Idx2D(Dimension - 2, 0)];
                for (int y = 0; y < Dimension; y++)
                {
                    Int2 globalPosition = new Int2(leftNeighbour.GlobalPosition.X + 16, leftNeighbour.GlobalPosition.Y + (y * 16));
                    Int2 localPosition = new Int2(Dimension - 1, y);

                    ChunkColumn column = new ChunkColumn(globalPosition, localPosition, minHeight, maxHeight);
                    for (int h = minHeight, localy = 0; h < maxHeight; h += 16, localy++)
                    {
                        Chunk chunk = new Chunk()
                        {
                            LocalPosition = new Int3(Dimension - 1, localy, y),
                            GlobalPosition = new Int3(globalPosition.X, h, globalPosition.Y),
                            
                            ChunkColumn =  column
                        };

                        column[localy] = chunk;
                    }

                    data[Idx2D(Dimension - 1, y)] = column;
                    
                    NoiseJob noiseJob = new NoiseJob()
                    {
                        Column = column
                    };
                    jobManager.Add(noiseJob);
                }

                for (int y = 0; y < Dimension; y++)
                {
                    for (int h = minHeight, localy = 0; h < maxHeight; h += 16, localy++)
                    {
                        data[Idx2D(0, y)][localy].ReleaseGameObject();
                    }

                    data[Idx2D(0, y)].State = DrawingState.NoiseReady;
                }
            }
        }

        #endregion
        
        public static Chunk GetChunk(Int3 local)
        {
            if (local.Y >= YBound || local.Y < 0)
                return null;

            lock (data[Idx2D(local.X, local.Z)])
            {
                return data[Idx2D(local.X, local.Z)][local.Y];
            }
        }

        public static Chunk GetChunkFromGlobal(int gx, int gy, int gz, Int2 playerPos)
        {
            if (gy >= YBound || gy < 0)
                return null;
            
            if (playerPos.X < 0) playerPos.X += 16;
            if (playerPos.Y < 0) playerPos.Y += 16;
            
            int xMin = playerPos.X - (DrawDistanceInChunks * chunkSize) - chunkSize;
            int zMin = playerPos.Y - (DrawDistanceInChunks * chunkSize) - chunkSize;
            
            int xMax = playerPos.X + (DrawDistanceInChunks * chunkSize) + chunkSize;
            int zMax = playerPos.Y + (DrawDistanceInChunks * chunkSize) + chunkSize;

            //Vielleicht -1 wieder weg??!??!?!
            int x = MathHelper.MapToInt(gx, xMin, xMax, 1, Dimension - 1);
            int z = MathHelper.MapToInt(gz, zMin, zMax, 1, Dimension - 1);
            
            int y = MathHelper.MapToInt(gy, minHeight, maxHeight, 0, YBound);

            lock (data[Idx2D(x, z)])
            {
                return data[Idx2D(x, z)][y];
            }
        }
        
        public static Chunk GetChunk(int x, int y, int z)
        {
            if (y >= YBound || y < 0)
                return null;

            lock (data[Idx2D(x, z)])
            {
                return data[Idx2D(x, z)][y];
            }
        }
        
        public static ChunkColumn GetChunkColumn(int x, int z)
        {
            lock (data[Idx2D(x, z)])
            {
                return data[Idx2D(x, z)];
            }
        }

        public static void SetChunkColumnNonThreadSafe(int x, int z, ChunkColumn value)
        {
            data[Idx2D(x, z)] = value;
        }


        public static int DataLength => data.Length;


        private static int Idx2D(int x, int y)
            => ((2 * DrawDistanceInChunks + 3)) * x + y;

        public static bool InLocalSpace(Int3 localPosition)
            => localPosition.X >= 0 && localPosition.X <= Dimension - 1 &&
               localPosition.Y >= 0 && localPosition.Y <= Dimension - 1 &&
               localPosition.Z >= 0 && localPosition.Z <= Dimension - 1;
    }
}