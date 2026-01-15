using UnityEngine;

namespace Assets.Scripts.Interfaces
{
    public interface IMovementStrategy
    {
        void Execute(Transform transform, float moveSpeed, float rotationSpeed, float deltaTime);
    }
}