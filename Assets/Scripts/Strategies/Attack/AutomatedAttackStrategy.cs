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
            // Simply check if cooldown has expired
            return _attackCooldown <= 0f;
        }

        public override void Execute(float deltaTime)
        {
            // Reset cooldown after attack is executed
            _attackCooldown = _attackInterval;
        }

        // Called every frame to update the cooldown timer
        public void UpdateCooldown(float deltaTime)
        {
            if (_attackCooldown > 0f)
            {
                _attackCooldown -= deltaTime;
            }
        }

        public void ResetCooldown()
        {
            _attackCooldown = _attackInterval;
        }

        public bool IsOnCooldown()
        {
            return _attackCooldown > 0f;
        }
    }
}