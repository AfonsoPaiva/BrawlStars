using UnityEngine;

namespace Assets.Scripts.Strategies.Movement
{
    public interface IMovementStrategy
    {
        void Execute(Transform transform, float moveSpeed, float rotationSpeed, float deltaTime);
    }
}