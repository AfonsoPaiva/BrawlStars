using UnityEngine;

namespace Assets.Scripts.Strategies
{
    public class FollowTheTankStrategy : MovementStrategyBase
    {
        private Transform _tank; // The player brawler to follow
        private float _followDistance = 2.0f; // How far behind the tank to stand
        private float _stopThreshold = 0.5f;  // Accuracy tolerance

        public FollowTheTankStrategy(Transform tank)
        {
            _tank = tank;
        }

        public override void Execute(Transform transform, float moveSpeed, float rotationSpeed, float deltaTime)
        {
            if (_tank == null) return;

            // 1. Calculate the target position: Behind the tank
            // Formula: TankPosition - (TankForward * Distance)
            Vector3 targetPosition = _tank.position - (_tank.forward * _followDistance);

            // 2. Calculate distance to that target spot
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

            if (distanceToTarget > _stopThreshold)
            {
                // --- PHASE 1: MOVING BEHAVIOR ---

                // Calculate move direction towards the target spot
                Vector3 directionToTarget = (targetPosition - transform.position).normalized;

                // Apply Position Movement
                transform.position += directionToTarget * moveSpeed * deltaTime;

                // Apply Rotation: Requirement says "look somewhat towards the tank"
                // So we look at the Tank itself, NOT the empty spot behind it.
                Vector3 directionToTank = (_tank.position - transform.position).normalized;

                // Zero out Y to prevent looking up/down (keep brawler flat)
                directionToTank.y = 0;

                if (directionToTank != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(directionToTank);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * deltaTime);
                }
            }
            else
            {
                // --- PHASE 2: REACHED BEHAVIOR ---

                // Requirement: "rotate until it looks in the same direction as the tank"
                // We simply Slerp towards the tank's rotation
                transform.rotation = Quaternion.Slerp(transform.rotation, _tank.rotation, rotationSpeed * deltaTime);
            }
        }
    }
}