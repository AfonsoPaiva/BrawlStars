using UnityEngine;

namespace Assets.Scripts.Strategies
{
    public class RotationalMovementStrategy : MovementStrategyBase
    {
        private readonly float _rotationSpeedOverride;

        public RotationalMovementStrategy(float rotationSpeed = 180f)
        {
            _rotationSpeedOverride = rotationSpeed;
        }

        public override void Execute(Transform transform, float moveSpeed, float rotationSpeed, float deltaTime)
        {
            // Continuous rotation around Y axis
            transform.Rotate(Vector3.up, _rotationSpeedOverride * deltaTime, Space.Self);
        }
    }
}