using UnityEngine;
using Assets.Scripts.Common;

namespace Assets.Scripts.Interfaces
{
    public interface IBullet
    {
        float Damage { get; }
        SerializableVector3 Position { get; }
    }
}
