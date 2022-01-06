using UnityEngine;

namespace VoxelWorld
{
    internal struct IntVector3
    {
        public int x;
        public int y;
        public int z;

        public IntVector3(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override bool Equals(object obj)
        {
            return obj is IntVector3 vector &&
                   x == vector.x &&
                   y == vector.y &&
                   z == vector.z;
        }

        public override int GetHashCode()
        {
            int hashCode = 373119288;
            hashCode = hashCode * -1521134295 + x.GetHashCode();
            hashCode = hashCode * -1521134295 + y.GetHashCode();
            hashCode = hashCode * -1521134295 + z.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(IntVector3 left, IntVector3 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(IntVector3 left, IntVector3 right)
        {
            return !(left == right);
        }

        public static IntVector3 operator +(IntVector3 left, IntVector3 right)
        {
            return new IntVector3(left.x + right.x, left.y + right.y, left.z + right.z);
        }

        public static IntVector3 operator -(IntVector3 left, IntVector3 right)
        {
            return new IntVector3(left.x - right.x, left.y - right.y, left.z - right.z);
        }

        public static IntVector3 operator *(IntVector3 left, IntVector3 right)
        {
            return new IntVector3(left.x * right.x, left.y * right.y, left.z * right.z);
        }

        public static IntVector3 operator /(IntVector3 left, IntVector3 right)
        {
            return new IntVector3(left.x / right.x, left.y / right.y, left.z / right.z);
        }

        public static IntVector3 operator *(IntVector3 left, int right)
        {
            return new IntVector3(left.x * right, left.y * right, left.z * right);
        }

        public static IntVector3 operator /(IntVector3 left, int right)
        {
            return new IntVector3(left.x / right, left.y / right, left.z / right);
        }

        public static Vector3 operator *(IntVector3 left, float right)
        {
            return new Vector3(left.x * right, left.y * right, left.z * right);
        }

        public static Vector3 operator /(IntVector3 left, float right)
        {
            return new Vector3(left.x / right, left.y / right, left.z / right);
        }

        public static implicit operator Vector3(IntVector3 vec)
        {
            return new Vector3(vec.x, vec.y, vec.z);
        }

        public override string ToString()
        {
            return $"({x},{y},{z})";
        }
    }
}
