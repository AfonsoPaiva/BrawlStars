using Assets.Scripts.Models;
using Assets.Scripts.Models.ColtModels;
using UnityEngine;

namespace Assets.Scripts.Strategies.Damage
{
    public class DistanceBasedDamageStrategy : DamageStrategyBase
    {
        private readonly float _maxDistance;
        private readonly float _minDamageMultiplier;

        public DistanceBasedDamageStrategy(float maxDistance = 10f, float minDamageMultiplier = 0.5f)
        {
            _maxDistance = maxDistance;
            _minDamageMultiplier = Mathf.Clamp01(minDamageMultiplier);
        }

        public override float CalculateDamage(Brawler target, Vector3 targetPosition, ColtBullet bullet)
        {
            if (target == null || bullet == null) return 0f;

            // Calculate distance using passed position and bullet position
            float distance = Vector3.Distance(bullet.Position, targetPosition);
            float damageMultiplier = Mathf.Lerp(1f, _minDamageMultiplier, distance / _maxDistance);
            
            return bullet.Damage * damageMultiplier;
        }
    }
}
