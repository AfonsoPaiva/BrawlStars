using UnityEngine.InputSystem;

namespace Assets.Scripts.Strategies.Attack
{
    public class InputSystemAttackStrategy : AttackStrategyBase
    {
        private readonly PlayerInput _playerInput;
        private InputAction _attackAction;
        private bool _attackRequested;

        public InputSystemAttackStrategy(PlayerInput playerInput)
        {
            _playerInput = playerInput;
            if (_playerInput != null)
            {
                _attackAction = _playerInput.actions["Attack"];
                _attackAction.performed += OnAttackPerformed;
            }
        }

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            _attackRequested = true;
        }

        public override bool CanExecute()
        {
            return _attackRequested;
        }

        public override void Execute(float deltaTime)
        {
            _attackRequested = false;
        }

        public void Cleanup()
        {
            if (_attackAction != null)
            {
                _attackAction.performed -= OnAttackPerformed;
            }
        }
    }
}
