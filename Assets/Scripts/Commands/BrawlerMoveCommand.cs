using Assets.Scripts.Interfaces;
using UnityEngine;
using System; // Required for Func

namespace Assets.Scripts.Commands
{
    public class BrawlerMoveCommand : ICommand
    {
        public float ExecutionTime { get; set; }

        public static Func<int, GameObject> FindGameObjectByID;

        private readonly int _modelID;
        private readonly Vector3 _position;
        private readonly Quaternion _rotation;

        public BrawlerMoveCommand(int modelID, Vector3 position, Quaternion rotation)
        {
            _modelID = modelID;
            _position = position;
            _rotation = rotation;
        }

        public void Execute()
        {
            // 2. Use the delegate instead of the Singleton
            if (FindGameObjectByID != null)
            {
                GameObject obj = FindGameObjectByID.Invoke(_modelID);

                if (obj != null)
                {
                    obj.transform.position = _position;
                    obj.transform.rotation = _rotation;
                }
            }
        }

        public void Reset() { }
    }
}