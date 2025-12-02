using Assets.Scripts.Models;
using Assets.Scripts.Models.ColtModels;
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

        public override float CalculateDamage(Brawler target, Vector3 targetPosition, ColtBullet bullet)
        {
            bool isCritical = Random.value <= _critChance;
            return isCritical ? bullet.Damage * _critMultiplier : bullet.Damage;
        }

    }
}
