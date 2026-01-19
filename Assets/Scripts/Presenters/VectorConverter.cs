using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.Common;

namespace Assets.Scripts.Presenters
{
    public static class VectorConverter
    {
        public static Vector3 ToUnity(SerializableVector3 sv3)
        {
            return new Vector3(sv3.X, sv3.Y, sv3.Z);
        }

        public static SerializableVector3 ToSerializable(Vector3 v3)
        {
            return new SerializableVector3(v3.x, v3.y, v3.z);
        }

        public static Quaternion ToUnity(SerializableQuaternion sq)
        {
            return new Quaternion(sq.X, sq.Y, sq.Z, sq.W);
        }

        public static SerializableQuaternion ToSerializable(Quaternion q)
        {
            return new SerializableQuaternion(q.x, q.y, q.z, q.w);
        }
    }

    [System.Serializable]
    public struct SerializableQuaternion
    {
        public float X, Y, Z, W;

        public SerializableQuaternion(float x, float y, float z, float w)
        {
            X = x; Y = y; Z = z; W = w;
        }
    }
}
