using Assets.Scripts.Models;
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

        public override float CalculateDamage(float baseDamage, Brawler target, GameObject sourceObject, GameObject targetObject)
        {
            if (sourceObject == null || targetObject == null) return baseDamage;

            float distance = Vector3.Distance(sourceObject.transform.position, targetObject.transform.position);
            float damageMultiplier = Mathf.Lerp(1f, _minDamageMultiplier, distance / _maxDistance);
            
            return baseDamage * damageMultiplier;
        }
    }
}
