using System;
using UnityEngine;

namespace Core.Math
{
    [Serializable]
    public struct Int3 : IntVector
    {
        public int X;
        public int Y;
        public int Z;
        
        public Int3(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public static Int3 operator +(Int3 target1, Int3 target2)
        {
            target1.X += target2.X;
            target1.Y += target2.Y;
            target1.Z += target2.Z;

            return target1;
        }

        public static Int3 operator -(Int3 target1, Int3 target2)
        {
            target1.X -= target2.X;
            target1.Y -= target2.Y;
            target1.Z -= target2.Z;

            return target1;
        }

        public static bool operator ==(Int3 target1, Int3 target2)
            => target1.X == target2.X && 
               target1.Y == target2.Y && 
               target1.Z == target2.Z;

        public static Int3 operator *(Int3 target, int scale)
        {
            target.X *= scale;
            target.Y *= scale;
            target.Z *= scale;

            return target;
        }

        public static Int3 operator /(Int3 target, int scale)
        {
            target.X /= scale;
            target.Y /= scale;
            target.Z /= scale;

            return target;
        }

        public static bool operator !=(Int3 target1, Int3 target2)
            => !(target1 == target2);

        public static Int3 Forward => new Int3(0, 0, 1);
        public static Int3 Back => new Int3(0, 0, -1);
        public static Int3 Up => new Int3(0, 1, 0);
        public static Int3 Down => new Int3(0, -1, 0);
        public static Int3 Left => new Int3(-1, 0, 0);
        public static Int3 Right => new Int3(1, 0, 0);
        public static Int3 One => new Int3(1, 1, 1);

        public static Int3 Zero => new Int3(0, 0, 0);

        public static Int3 ToInt3(Vector3 target)
            => new Int3(Mathf.RoundToInt(target.x), Mathf.RoundToInt(target.y), Mathf.RoundToInt(target.z));
        
        public static Int3 ToInt3(Vector3Int target)
            => new Int3(target.x, target.y, target.z);

        public static Int3 FloorToInt(Vector3 target)
            => new Int3(Mathf.FloorToInt(target.x), Mathf.FloorToInt(target.y), Mathf.FloorToInt(target.z));

        public Vector3 ToVector3()
            => new Vector3Int(this.X, this.Y, this.Z);

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 47;
                hash = hash * 227 + this.X.GetHashCode();
                hash = hash * 227 + this.Y.GetHashCode();
                hash = hash * 227 + this.Z.GetHashCode();

                return hash;
            }
        }

        public override string ToString()
            => $"({this.X}, {this.Y}, {this.Z})";

        /// <summary>
        /// Determines if any of the attribute values is true to the given predicate
        /// </summary>
        /// <param name="predicate">Predicate for each dimension in the int3 vector</param>
        /// <param name="value">The Dimension in which the predicate is true</param>
        /// <returns>Is any of the dimensions affected by the predicate</returns>
        public bool AnyAttribute(Predicate<int> predicate, out int value)
        {
            value = -1;
            bool xResult = predicate(this.X); // wenn wahr, dann true z.b 16 > 15? true
            bool yResult = predicate(this.Y);
            bool zResult = predicate(this.Z);
            

            if (xResult)
                value = 0;
            else if (yResult)
                value = 1;
            else if (zResult)
                value = 2;

            return (xResult || yResult || zResult);
        }


        /// <summary>
        /// Determines if any of the attribute values is true to the given predicate
        /// </summary>
        /// <param name="predicate">Predicate for each dimension in the int3 vector</param>
        /// <param name="mask">The Dimension in which the predicate is true</param>
        /// <returns>Is any of the dimensions affected by the predicate</returns>
        public bool AnyAttributeMasked(Predicate<int> predicate, out int mask)
        {
            mask = 0b000;
            var xResult = predicate(this.X); // wenn wahr, dann true z.b 16 > 15? true
            var yResult = predicate(this.Y);
            var zResult = predicate(this.Z);

            if (xResult)
                mask |= 1 << 0;
            if (yResult)
                mask |= 1 << 1;
            if (zResult)
                mask |= 1 << 2;

            return (xResult || yResult || zResult);
        }
    }
}
