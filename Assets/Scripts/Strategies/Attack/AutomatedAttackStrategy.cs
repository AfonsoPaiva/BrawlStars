using UnityEngine;

namespace Assets.Scripts.Strategies
{
    public class AutomatedAttackStrategy : AttackStrategyBase
    {
        public AutomatedAttackStrategy(float attackInterval = 1f) : base(attackInterval) { }

        public override bool CanExecute()
        {
            return _attackCooldown <= 0f; // Always attack when ready
        }
    }
}