using System;
using UnityEngine;

namespace Core.Math
{
    [Serializable]
    public struct Int2 : IntVector
    {
        public int X;
        public int Y;

        public static Int2 One => new Int2(1, 1);
        public static Int2 Zero = new Int2(0, 0);
        public static Int2 Forward => new Int2(0, 1);
        public static Int2 Back => new Int2(0, -1);
        public static Int2 Left => new Int2(-1, 0);
        public static Int2 Right => new Int2(1, 0);

        public Int2(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
        public static Int2 operator +(Int2 target1, Int2 target2)
        {
            target1.X += target2.X;
            target1.Y += target2.Y;

            return target1;
        }

        public static Int2 operator -(Int2 target1, Int2 target2)
        {
            target1.X -= target2.X;
            target1.Y -= target2.Y;

            return target1;
        }

        public static bool operator ==(Int2 target1, Int2 target2)
            => target1.X == target2.X && target1.Y == target2.Y;

        public static Int2 operator *(Int2 target, int scale)
        {
            target.X *= scale;
            target.Y *= scale;
            return target;
        }

        public static Int2 operator /(Int2 target, int scale)
        {
            target.X /= scale;
            target.Y /= scale;
            return target;
        }

        public static bool operator !=(Int2 target1, Int2 target2)
            => !(target1 == target2);

        public static Int2 ToInt2(Vector2 target)
            => new Int2(Mathf.RoundToInt(target.x), Mathf.RoundToInt(target.y));
        
        public static Int2 ToInt3(Vector2Int target)
            => new Int2(target.x, target.y);

        public static Int2 FloorToInt(Vector2 target)
            => new Int2(Mathf.FloorToInt(target.x), Mathf.FloorToInt(target.y));

        public Vector2 ToVector3()
            => new Vector2Int(this.X, this.Y);

        public override bool Equals(object obj)
            => base.Equals(obj);

        public override int GetHashCode()
            => base.GetHashCode();

        public override string ToString()
            => $"({this.X}, {this.Y})";

        public bool AnyAttribute(Predicate<int> predicate, out int value)
        {
            value = -1;
            bool xResult = predicate(this.X); // wenn wahr, dann true z.b 16 > 15? true
            bool yResult = predicate(this.Y);

            if (xResult)
                value = 0;
            else if (yResult)
                value = 1;

            return (xResult || yResult);
        }
    }
}
