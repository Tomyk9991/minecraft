using System;

namespace Utilities
{
    public struct Masker32
    {
        private uint _data;
        private static readonly byte SIZE = 32;

        public Masker32(uint data)
        {
            this._data = data;
        }

        public void ForEach(Action<bool> action)
        {
            for (int i = 0; i < SIZE; i++)
                action.Invoke(GetAtFast(i));
        }

        public void ForEachIndex(Action<bool, int> action)
        {
            for (int i = 0; i < SIZE; i++)
                action.Invoke(GetAtFast(i), i);
        }

        private bool GetAtFast(int index)
            => (_data & 1 << index) != 0;

        public static Masker32 operator <<(Masker32 mask, int value)
        {
            mask._data = 1;
            mask._data <<= value;
            return mask;
        }


        public static bool operator ==(Masker32 mask1, Masker32 mask2)
            => mask1._data == mask2._data;

        public static bool operator !=(Masker32 mask1, Masker32 mask2)
            => mask1._data != mask2._data;

        public static bool operator ==(Masker32 mask1, uint mask2)
            => mask1._data == mask2;

        public static bool operator !=(Masker32 mask1, uint mask2)
            => mask1._data != mask2;

        public static bool operator ==(uint mask1, Masker32 mask2)
            => mask1 == mask2._data;

        public static bool operator !=(uint mask1, Masker32 mask2)
            => !(mask2 == mask1);

        public bool this[int index]
        {
            get
            {
                if (index < 0 || index >= SIZE)
                    throw new IndexOutOfRangeException("You need an index between 0 and 8");

                return (_data & 1 << index) != 0;
            }
            set
            {
                if (index < 0 || index >= SIZE)
                    throw new IndexOutOfRangeException("You need an index between 0 and 8");

                _data = value ? (uint)(_data | (uint)(1 << index)) : (uint)(_data & (uint)~(1 << index));
            }
        }

        public uint ToUInt()
            => _data;

        public override string ToString()
            => _data.ToString();

        public override bool Equals(object obj)
        {
            return obj is Masker32 masker && _data == masker._data;
        }

        public override int GetHashCode()
        {
            return _data.GetHashCode();
        }
    }
}

