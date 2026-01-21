using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Strategies
{
    public class InputSystemAttackStrategy : AttackStrategyBase
    {
        private readonly PlayerInput _playerInput;
        private InputAction _attackAction;
        private bool _attackRequested;

        public InputSystemAttackStrategy(PlayerInput playerInput, float attackInterval = 0.5f) 
            : base(attackInterval)
        {
            _playerInput = playerInput;
            _attackRequested = false;

            if (_playerInput != null && _playerInput.actions != null)
            {
                _attackAction = _playerInput.actions.FindAction("Attack", false);
                if (_attackAction != null)
                {
                    _attackAction.performed += OnAttackPerformed;
                }
            }
        }

        private void OnAttackPerformed(InputAction.CallbackContext ctx)
        {
            _attackRequested = true;
        }

        public override bool CanExecute()
        {
            bool pressed = (_attackAction != null && _attackAction.IsPressed()) || _attackRequested;
            return pressed && _attackCooldown <= 0f;
        }

        public override void Execute(float deltaTime)
        {
            base.Execute(deltaTime); // Calls base to reset cooldown
            _attackRequested = false;
        }

        public override void Cleanup()
        {
            if (_attackAction != null)
            {
                _attackAction.performed -= OnAttackPerformed;
            }
        }
    }
}