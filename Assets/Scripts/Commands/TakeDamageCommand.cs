using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;
using UnityEngine;

namespace Assets.Scripts.Commands
{
    public class TakeDamageCommand : ICommand
    {
        public float ExecutionTime { get; set; }
        
        private readonly int _targetModelID;
        private readonly float _damage;
        private readonly PD3StarsGame _gameModel;

        public TakeDamageCommand(int targetModelID, float damage, PD3StarsGame gameModel)
        {
            _targetModelID = targetModelID;
            _damage = damage;
            _gameModel = gameModel;
        }

        public void Execute()
        {
            // Find brawler by ModelID
            Brawler target = FindBrawlerByModelID(_targetModelID);
            
            if (target != null)
            {
                target.TakeDamage(_damage);
                Debug.Log($"TakeDamageCommand executed: ModelID={_targetModelID}, Damage={_damage}");
            }
            else
            {
                Debug.LogWarning($"TakeDamageCommand: Could not find brawler with ModelID={_targetModelID}");
            }
        }

        public void Reset()
        {
            // No state to reset for this command
        }

        private Brawler FindBrawlerByModelID(int modelID)
        {
            foreach (var brawler in _gameModel.Brawlers)
            {
                if (brawler.ModelID == modelID)
                {
                    return brawler;
                }
            }
            return null;
        }
    }
}
