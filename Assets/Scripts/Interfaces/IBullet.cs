using UnityEngine;

namespace Assets.Scripts.Interfaces
{
    public interface IBullet
    {
        float Damage { get; }
        Vector3 Position { get; }
    }
}
