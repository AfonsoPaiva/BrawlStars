using UnityEngine;

namespace Assets.Scripts.Strategies
{
    public class PlayerMovementStrategy : MovementStrategyBase
    {
        public override void Execute(Transform transform, float moveSpeed, float rotationSpeed, float deltaTime)
        {
            // Read input directly from Unity Input system
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            
            _moveDirection = new Vector2(horizontal, vertical);
            
            ApplyMovement(transform, moveSpeed, rotationSpeed, deltaTime);
        }
    }
}