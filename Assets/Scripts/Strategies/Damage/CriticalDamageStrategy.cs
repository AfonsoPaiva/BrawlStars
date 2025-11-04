using Assets.Scripts.Models;
using UnityEngine;

namespace Assets.Scripts.Strategies.Damage
{

    public class CriticalDamageStrategy : DamageStrategyBase
    {
        private readonly float _critChance;
        private readonly float _critMultiplier;

        public CriticalDamageStrategy(float critChance = 0.2f, float critMultiplier = 2f)
        {
            _critChance = Mathf.Clamp01(critChance);
            _critMultiplier = critMultiplier;
        }

        public override float CalculateDamage(float baseDamage, Brawler target, GameObject sourceObject, GameObject targetObject)
        {
            bool isCritical = Random.value <= _critChance;
            return isCritical ? baseDamage * _critMultiplier : baseDamage;
        }
    }
}
