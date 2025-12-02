using Assets.Scripts.Models;
using Assets.Scripts.Models.ColtModels;
using UnityEngine;

namespace Assets.Scripts.Strategies.Damage
{
    public abstract class DamageStrategyBase : IDamageStrategy
    {
        public abstract float CalculateDamage(Brawler target, Vector3 targetPosition, ColtBullet bullet);

        public virtual void ApplyDamage(Brawler target, Vector3 targetPosition, ColtBullet bullet)
        {
            if (target == null || bullet == null) return;
            
            float damage = CalculateDamage(target, targetPosition, bullet);
            target.TakeDamage(damage);
        }
    }
}
