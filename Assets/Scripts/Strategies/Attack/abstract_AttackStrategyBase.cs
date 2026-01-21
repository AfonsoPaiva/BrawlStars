using UnityEngine;
using Assets.Scripts.Interfaces;

namespace Assets.Scripts.Strategies
{
    public abstract class AttackStrategyBase : IAttackStrategy
    {
        protected float _attackCooldown;
        protected readonly float _attackInterval;

        protected AttackStrategyBase(float attackInterval = 0.5f)
        {
            _attackInterval = attackInterval;
            _attackCooldown = 0f;
        }

        public abstract bool CanExecute();

        public virtual void Execute(float deltaTime)
        {
            _attackCooldown = _attackInterval; // Reset cooldown
        }

        public virtual void UpdateCooldown(float deltaTime)
        {
            if (_attackCooldown > 0f)
            {
                _attackCooldown -= deltaTime;
                if (_attackCooldown < 0f) _attackCooldown = 0f;
            }
        }

        public virtual void Cleanup() { } // Default empty implementation

        public float AttackCooldown => _attackCooldown;
        public float AttackInterval => _attackInterval;

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