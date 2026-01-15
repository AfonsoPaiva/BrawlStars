using Assets.Scripts.Interfaces;
using UnityEngine;

namespace Assets.Scripts.Strategies
{
    public class CriticalDamageStrategy : DamageStrategyBase
    {
        private readonly float _criticalMultiplier;
        private readonly float _criticalRange;

        public CriticalDamageStrategy(float criticalMultiplier = 2.0f, float criticalRange = 1.0f)
        {
            _criticalMultiplier = criticalMultiplier;
            _criticalRange = criticalRange;
        }

        public override float CalculateDamage(IBrawler target, Vector3 targetPosition, IBullet bullet)
        {
            if (target == null || bullet == null) return 0f;

            float distance = Vector3.Distance(bullet.Position, targetPosition);
            float baseDamage = bullet.Damage;
            return (distance <= _criticalRange) ? baseDamage * _criticalMultiplier : baseDamage;
        }
    }
}