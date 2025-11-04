using Assets.Scripts.Models;
using UnityEngine;

namespace Assets.Scripts.Strategies.Damage
{

    public class StandardDamageStrategy : DamageStrategyBase
    {
        public override float CalculateDamage(float baseDamage, Brawler target, GameObject sourceObject, GameObject targetObject)
        {
            return baseDamage;
        }
    }
}
