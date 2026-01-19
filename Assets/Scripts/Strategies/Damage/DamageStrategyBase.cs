using Assets.Scripts.Interfaces;
using Assets.Scripts.Common;

namespace Assets.Scripts.Strategies
{
    public abstract class DamageStrategyBase : IDamageStrategy
    {
        public abstract float CalculateDamage(IBrawler target, SerializableVector3 targetPosition, IBullet bullet);

        public virtual void ApplyDamage(IBrawler target, SerializableVector3 targetPosition, IBullet bullet)
        {
            if (target == null || bullet == null) return;
            
            float damage = CalculateDamage(target, targetPosition, bullet);
            target.TakeDamage(damage);
        }
    }
}
