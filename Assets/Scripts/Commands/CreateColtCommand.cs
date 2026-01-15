using System;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;
using UnityEngine;

namespace Assets.Scripts.Commands
{
    public class CreateColtCommand : ICommand
    {
        // Use shared counter from BrawlerCommandHelper
        public float ExecutionTime { get; set; }
        
        private readonly PD3StarsGame _gameModel;
        private readonly Vector3 _spawnPosition;
        private readonly Quaternion _spawnRotation;
        private readonly bool _isLocalPlayer;
        private int _assignedModelID;

        public Vector3 SpawnPosition => _spawnPosition;
        public Quaternion SpawnRotation => _spawnRotation;

        public CreateColtCommand(PD3StarsGame gameModel, Vector3 spawnPosition, Quaternion spawnRotation, bool isLocalPlayer = false)
        {
            _gameModel = gameModel;
            _spawnPosition = spawnPosition;
            _spawnRotation = spawnRotation;
            _isLocalPlayer = isLocalPlayer;
        }

        public void Execute()
        {
            // Create Colt model with predictable ID
            Colt colt = new Colt();
            
            // Assign consistent ModelID using shared counter
            _assignedModelID = BrawlerCommandHelper.GetNextBrawlerID();
            colt.SetModelID(_assignedModelID);

            // CRITICAL: Register command BEFORE AddBrawler triggers OnBrawlerAdded event
            CommandRegistryLocator.Registry?.RegisterCommandForModel(_assignedModelID, this);
            
            // Add to game (this triggers OnBrawlerAdded which needs the command registered)
            _gameModel.AddBrawler(colt, _isLocalPlayer);
            
            Debug.Log($"CreateColtCommand executed: ModelID={_assignedModelID}, Position={_spawnPosition}, IsLocal={_isLocalPlayer}");
        }

        public void Reset()
        {
            // Reset is called per-instance during replay if needed
        }

        public int GetAssignedModelID() => _assignedModelID;
    }
}
