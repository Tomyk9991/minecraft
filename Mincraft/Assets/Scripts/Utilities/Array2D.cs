using System;
using System.Linq;

namespace Utilities
{
    public class Array2D<T>
    {
        public int Length => data.Length;
        public int Width => width;
        public int Height => height;
        public T[] Data => data;
        
        private T[] data;
        private readonly int width;
        private readonly int height;
        
        public Array2D(int sizeD)
        {
            this.width = sizeD;
            this.height = sizeD;
            data = new T[sizeD * sizeD];
        }
        
        public Array2D(int width, int height)
        {
            this.width = width;
            this.height = height;
            data = new T[width * height];
        }

        public T this[int x, int y]
        {
            get => data[Idx2D(x, y)];
            set => data[Idx2D(x, y)] = value;
        }

        public T this[int i]
        {
            get => data[i];
            set => data[i] = value;
        }

        public void Clear()
        {
            this.data = new T[width * height];
        }

        private int Idx2D(int x, int y)
            => width * x + y;

        public override string ToString()
        {
            int counter = data.Sum(t => t == null ? 0 : 1);
            return counter.ToString();
        }

        
        /// <summary>Returns a copy of the internal array</summary>
        public T[] ToArray()
        {
            T[] newArray = new T[data.Length];
            Array.Copy(data, newArray, data.Length);

            return newArray;
        }
    }
    
    public class ExtendedArray2D<T> : Array2D<T>
    {
        public ExtendedArray2D(int sizeD, int extension) : base(sizeD + (extension * 2))
        { }

        public new T this[int x, int y]
        {
            //Map [(-1, -1), (16, 16)] to [(0, 0), (17, 17)];
            get => base[x + 1, y + 1];
            set => base[x + 1, y + 1] = value;
        }
    }
}