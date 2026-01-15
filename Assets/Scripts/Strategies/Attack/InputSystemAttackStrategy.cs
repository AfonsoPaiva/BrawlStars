using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Strategies
{
    public class InputSystemAttackStrategy : AttackStrategyBase
    {
        private readonly PlayerInput _playerInput;
        private InputAction _attackAction;
        private float _attackCooldown;
        private readonly float _attackInterval;
        private bool _attackRequested;

        public InputSystemAttackStrategy(PlayerInput playerInput, float attackInterval = 0.5f)
        {
            _playerInput = playerInput;
            _attackInterval = attackInterval;
            _attackCooldown = 0f;
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
            // allow when input requested or held and cooldown expired
            bool pressed = (_attackAction != null && _attackAction.IsPressed()) || _attackRequested;
            return pressed && _attackCooldown <= 0f;
        }

        public override void Execute(float deltaTime)
        {
            // Reset cooldown after attack is executed and clear request flag
            _attackCooldown = _attackInterval;
            _attackRequested = false;
        }

        // Called every frame by presenter
        public override void UpdateCooldown(float deltaTime)
        {
            if (_attackCooldown > 0f)
            {
                _attackCooldown -= deltaTime;
                if (_attackCooldown < 0f) _attackCooldown = 0f;
            }
        }

        public override void Cleanup()
        {
            if (_attackAction != null)
            {
                _attackAction.performed -= OnAttackPerformed;
            }
        }

        // Expose cooldown/interval and PA progress
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