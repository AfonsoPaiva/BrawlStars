using UnityEngine;

namespace Assets.Scripts.Strategies
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
        public override void UpdateCooldown(float deltaTime)
        {
            if (_attackCooldown > 0f)
            {
                _attackCooldown -= deltaTime;
                if (_attackCooldown < 0f) _attackCooldown = 0f;
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

        // Expose cooldown/interval so presenters/models can compute PA progress
        public float AttackCooldown => _attackCooldown;
        public float AttackInterval => _attackInterval;

        // PA progress in range [0,1] where 1 == ready
        public float PAProgress
        {
            get
            {
                if (_attackInterval <= 0f) return 1f;
                return Mathf.Clamp01(1f - (_attackCooldown / _attackInterval));
            }
        }
    }
}