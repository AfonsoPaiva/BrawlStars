using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Strategies.Attack
{
    public class InputSystemAttackStrategy : AttackStrategyBase
    {
        private readonly PlayerInput _playerInput;
        private InputAction _attackAction;
        private float _attackCooldown;
        private readonly float _attackInterval;

        public InputSystemAttackStrategy(PlayerInput playerInput, float attackInterval = 0.5f)
        {
            _playerInput = playerInput;
            _attackInterval = attackInterval;
            _attackCooldown = 0f;

            if (_playerInput != null)
            {
                _attackAction = _playerInput.actions["Attack"];
            }
        }

        public override bool CanExecute()
        {
            // Check if the attack button is currently pressed (held down) and cooldown has expired
            if (_attackAction != null && _attackAction.IsPressed() && _attackCooldown <= 0f)
            {
                return true;
            }
            return false;
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

        public void Cleanup()
        {
            // No event subscriptions to clean up anymore
        }
    }
}