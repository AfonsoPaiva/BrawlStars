    using Assets.Scripts.Models;
using Assets.Scripts.Models.ColtModels;
using UnityEngine;

namespace Assets.Scripts.Strategies.Damage
{
    public class StandardDamageStrategy : DamageStrategyBase
    {
        public override float CalculateDamage(Brawler target, Vector3 targetPosition, ColtBullet bullet)
        {
            return bullet.Damage;
        }
    }
}
