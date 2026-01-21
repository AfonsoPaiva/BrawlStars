using UnityEngine;

namespace Assets.Scripts.Strategies
{
    public class FollowTheTankStrategy : MovementStrategyBase
    {
        private Transform _tank;
        private float _followDistance = 2.0f;
        private float _stopThreshold = 0.5f;

        public FollowTheTankStrategy(Transform tank)
        {
            _tank = tank;
        }

        public override void Execute(Transform transform, float moveSpeed, float rotationSpeed, float deltaTime)
        {
            if (_tank == null)
            {
                _moveDirection = Vector2.zero; // Stop moving if target is lost
                return;
            }

            Vector3 targetPosition = _tank.position - (_tank.forward * _followDistance);
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

            if (distanceToTarget > _stopThreshold)
            {
                // Calculate direction and set _moveDirection (like user input does)
                Vector3 directionToTarget = (targetPosition - transform.position).normalized;
                _moveDirection = new Vector2(directionToTarget.x, directionToTarget.z);

                // Use inherited ApplyMovement for consistency
                ApplyMovement(transform, moveSpeed, rotationSpeed, deltaTime);
            }
            else
            {
                // When reached, align rotation with tank
                _moveDirection = Vector2.zero; // Stop moving
                transform.rotation = Quaternion.Slerp(transform.rotation, _tank.rotation, rotationSpeed * deltaTime);
            }
        }
    }
}