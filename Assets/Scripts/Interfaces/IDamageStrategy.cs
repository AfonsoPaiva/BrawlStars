using UnityEngine;

namespace Assets.Scripts.Interfaces
{
    public interface IDamageStrategy
    {
        float CalculateDamage(IBrawler target, Vector3 targetPosition, IBullet bullet);
        void ApplyDamage(IBrawler target, Vector3 targetPosition, IBullet bullet);
    }
}
