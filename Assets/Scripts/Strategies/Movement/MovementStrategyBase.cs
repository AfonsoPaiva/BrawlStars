using UnityEngine;
using Assets.Scripts.Interfaces;

namespace Assets.Scripts.Strategies
{
    public abstract class MovementStrategyBase : IMovementStrategy
    {
        protected Vector2 _moveDirection;

        public abstract void Execute(Transform transform, float moveSpeed, float rotationSpeed, float deltaTime);

        protected void ApplyMovement(Transform transform, float moveSpeed, float rotationSpeed, float deltaTime)
        {
            if (_moveDirection.sqrMagnitude > 0.01f)
            {
                Vector3 movement = new Vector3(_moveDirection.x, 0, _moveDirection.y);
                transform.position += movement * moveSpeed * deltaTime;

                Quaternion targetRotation = Quaternion.LookRotation(movement);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * deltaTime);
            }
        }
    }
}
