namespace Utilities
{
    public class Array2D<T>
    {
        public int Length => data.Length;
        private T[] data;
        private readonly int width;
        
        public Array2D(int sizeD)
        {
            this.width = sizeD;
            data = new T[sizeD * sizeD];
        }

        public T this[int x, int y]
        {
            get => data[Idx2D(x, y)];
            set => data[Idx2D(x, y)] = value;
        }

        public void Clear()
        {
            this.data = new T[width * width];
        }

        private int Idx2D(int x, int y)
            => width * x + y;
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