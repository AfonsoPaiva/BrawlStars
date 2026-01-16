using System;

namespace Assets.Scripts.Models
{
    /// <summary>
    /// Unity-independent 3D vector representation
    /// Allows Model layer to work with 3D positions without UnityEngine dependency
    /// </summary>
    [Serializable]
    public struct SerializableVector3
    {
        public float X;
        public float Y;
        public float Z;

        public SerializableVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static SerializableVector3 Zero => new SerializableVector3(0, 0, 0);
        public static SerializableVector3 One => new SerializableVector3(1, 1, 1);
        public static SerializableVector3 Forward => new SerializableVector3(0, 0, 1);

        public float Magnitude => (float)Math.Sqrt(X * X + Y * Y + Z * Z);

        public SerializableVector3 Normalized
        {
            get
            {
                float mag = Magnitude;
                if (mag > 0.00001f)
                    return new SerializableVector3(X / mag, Y / mag, Z / mag);
                return Zero;
            }
        }

        public static SerializableVector3 operator +(SerializableVector3 a, SerializableVector3 b)
        {
            return new SerializableVector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static SerializableVector3 operator -(SerializableVector3 a, SerializableVector3 b)
        {
            return new SerializableVector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static SerializableVector3 operator *(SerializableVector3 a, float scalar)
        {
            return new SerializableVector3(a.X * scalar, a.Y * scalar, a.Z * scalar);
        }

        public static SerializableVector3 Lerp(SerializableVector3 a, SerializableVector3 b, float t)
        {
            t = Math.Max(0f, Math.Min(1f, t)); // Clamp between 0 and 1
            return new SerializableVector3(
                a.X + (b.X - a.X) * t,
                a.Y + (b.Y - a.Y) * t,
                a.Z + (b.Z - a.Z) * t
            );
        }

        public override string ToString()
        {
            return $"({X:F2}, {Y:F2}, {Z:F2})";
        }
    }
}