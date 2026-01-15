using Assets.Scripts.Interfaces;
using UnityEngine;

namespace Assets.Scripts.Strategies
{
    public class StandardDamageStrategy : DamageStrategyBase
    {
        public override float CalculateDamage(IBrawler target, Vector3 targetPosition, IBullet bullet)
        {
            return bullet.Damage;
        }
    }
}
