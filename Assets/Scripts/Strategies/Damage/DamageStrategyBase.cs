using UnityEngine;
using Assets.Scripts.Interfaces;

namespace Assets.Scripts.Strategies
{
    public abstract class DamageStrategyBase : IDamageStrategy
    {
        public abstract float CalculateDamage(IBrawler target, Vector3 targetPosition, IBullet bullet);

        public virtual void ApplyDamage(IBrawler target, Vector3 targetPosition, IBullet bullet)
        {
            if (target == null || bullet == null) return;
            
            float damage = CalculateDamage(target, targetPosition, bullet);
            target.TakeDamage(damage);
        }
    }
}
