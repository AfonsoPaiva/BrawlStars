using UnityEngine;

namespace Assets.Scripts.Strategies.Movement
{
    public class NoMovementStrategy : MovementStrategyBase
    {
        public override void Execute(Transform transform, float moveSpeed, float rotationSpeed, float deltaTime)
        {
            // Intentionally does nothing - brawler stays still
        }
    }
}