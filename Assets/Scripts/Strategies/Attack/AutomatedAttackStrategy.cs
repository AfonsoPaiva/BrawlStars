using UnityEngine;

namespace Assets.Scripts.Strategies.Attack
{
    public class AutomatedAttackStrategy : AttackStrategyBase
    {
        private float _attackCooldown;
        private readonly float _attackInterval;

        public AutomatedAttackStrategy(float attackInterval = 1f)
        {
            _attackInterval = attackInterval;
            _attackCooldown = 0f;
        }

        public override bool CanExecute()
        {
            // Update cooldown first
            if (_attackCooldown > 0f)
            {
                _attackCooldown -= Time.deltaTime;
            }

            return _attackCooldown <= 0f;
        }

        public override void Execute(float deltaTime)
        {
            // Attack logic - just reset the cooldown
            ResetCooldown();
        }

        public void ResetCooldown()
        {
            _attackCooldown = _attackInterval;
        }

        // Optional: Add a method to check current cooldown state
        public bool IsOnCooldown()
        {
            return _attackCooldown > 0f;
        }
    }
}