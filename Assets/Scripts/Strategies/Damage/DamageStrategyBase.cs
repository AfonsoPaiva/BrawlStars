using Assets.Scripts.Models;
using UnityEngine;

namespace Assets.Scripts.Strategies.Damage
{
    public abstract class DamageStrategyBase : IDamageStrategy
    {
        public virtual void ApplyDamage(Brawler target, float baseDamage, GameObject sourceObject, GameObject targetObject)
        {
            if (target == null) return;

            float finalDamage = CalculateDamage(baseDamage, target, sourceObject, targetObject);
            target.TakeDamage(finalDamage);
        }

        public abstract float CalculateDamage(float baseDamage, Brawler target, GameObject sourceObject, GameObject targetObject);
    }
}
