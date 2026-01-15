using Assets.Scripts.Interfaces;
using UnityEngine;

namespace Assets.Scripts.Strategies
{
    public class DistanceBasedDamageStrategy : DamageStrategyBase
    {
        private readonly float _maxRange;
        private readonly float _minDamageMultiplier;

        public DistanceBasedDamageStrategy(float maxRange = 10f, float minDamageMultiplier = 0.5f)
        {
            _maxRange = maxRange;
            _minDamageMultiplier = minDamageMultiplier;
        }

        public override float CalculateDamage(IBrawler target, Vector3 targetPosition, IBullet bullet)
        {
            if (target == null || bullet == null) return 0f;

            float distance = Vector3.Distance(bullet.Position, targetPosition);
            float t = Mathf.Clamp01(distance / _maxRange);
            float multiplier = Mathf.Lerp(1f, _minDamageMultiplier, t);
            return bullet.Damage * multiplier;
        }
    }
}