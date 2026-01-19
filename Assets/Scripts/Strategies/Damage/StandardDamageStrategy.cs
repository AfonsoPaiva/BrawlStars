using Assets.Scripts.Interfaces;
using Assets.Scripts.Common;

namespace Assets.Scripts.Strategies
{   
    public class StandardDamageStrategy : DamageStrategyBase
    {
        public override float CalculateDamage(IBrawler target, SerializableVector3 targetPosition, IBullet bullet)
        {
            return bullet?.Damage ?? 0f;
        }
    }
}
