using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Strategies.Movement
{
    public class InputSystemMovementStrategy : MovementStrategyBase
    {
        private readonly PlayerInput _playerInput;
        private InputAction _moveAction;

        public InputSystemMovementStrategy(PlayerInput playerInput)
        {
            _playerInput = playerInput;
            if (_playerInput != null)
            {
                _moveAction = _playerInput.actions["Move"];
                _moveAction.performed += OnMovePerformed;
                _moveAction.canceled += OnMoveCanceled;
            }
        }

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            _moveDirection = context.ReadValue<Vector2>();
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            _moveDirection = Vector2.zero;
        }

        public override void Execute(Transform transform, float moveSpeed, float rotationSpeed, float deltaTime)
        {
            ApplyMovement(transform, moveSpeed, rotationSpeed, deltaTime);
        }

        public void Cleanup()
        {
            if (_moveAction != null)
            {
                _moveAction.performed -= OnMovePerformed;
                _moveAction.canceled -= OnMoveCanceled;
            }
        }
    }
}
