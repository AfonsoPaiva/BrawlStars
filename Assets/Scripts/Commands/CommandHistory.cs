using System;
using System.Collections.Generic;
using Assets.Scripts.Interfaces;
using UnityEngine;
using Assets.Scripts.Models;

namespace Assets.Scripts.Commands
{
    public class CommandHistory
    {
        private readonly List<ICommand> _commandHistory = new List<ICommand>();
        private float _sessionStartTime;
        private int _replayIndex = 0;
        
        // Random seed for consistent replays
        public int RandomSeed { get; private set; }
        public System.Random Random { get; private set; }

        public IReadOnlyList<ICommand> Commands => _commandHistory.AsReadOnly();
        public bool IsReplaying { get; private set; }

        public CommandHistory(int? seed = null)
        {
            RandomSeed = seed ?? UnityEngine.Random.Range(0, int.MaxValue);
            Random = new System.Random(RandomSeed);
            _sessionStartTime = Time.time;
        }

        /// <summary>
        /// Execute a command immediately and add it to history
        /// </summary>
        public void ExecuteCommand(ICommand command)
        {
            if (IsReplaying)
            {
                Debug.LogWarning("Cannot execute new commands during replay!");
                return;
            }

            // Set execution time relative to session start
            command.ExecutionTime = Time.time - _sessionStartTime;
            
            // Execute the command
            command.Execute();
            
            // Store in history
            _commandHistory.Add(command);
            
            Debug.Log($"Executed command at time {command.ExecutionTime:F3}s: {command.GetType().Name}");
        }

        /// <summary>
        /// Start replay mode
        /// </summary>
        public void StartReplay()
        {
            IsReplaying = true;
            _replayIndex = 0;
            _sessionStartTime = Time.time;
            
            Debug.Log($"Starting replay with {_commandHistory.Count} commands and seed {RandomSeed}");
        }

        /// <summary>
        /// Execute commands up to the current replay time
        /// </summary>
        public void ReplayUntil(float replayTime)
        {
            if (!IsReplaying)
            {
                Debug.LogWarning("Not in replay mode!");
                return;
            }

            while (_replayIndex < _commandHistory.Count)
            {
                ICommand command = _commandHistory[_replayIndex];
                
                if (command.ExecutionTime > replayTime)
                {
                    // Haven't reached this command's execution time yet
                    break;
                }

                // Execute the command
                command.Execute();
                Debug.Log($"Replayed command at {command.ExecutionTime:F3}s: {command.GetType().Name}");
                
                _replayIndex++;
            }

            // Check if replay is complete
            if (_replayIndex >= _commandHistory.Count)
            {
                Debug.Log("Replay completed!");
                IsReplaying = false;
            }
        }

        /// <summary>
        /// Stop replay mode
        /// </summary>
        public void StopReplay()
        {
            IsReplaying = false;
            _replayIndex = 0;
            Debug.Log("Replay stopped");
        }

        /// <summary>
        /// Clear all command history
        /// </summary>
        public void Clear()
        {
            _commandHistory.Clear();
            _replayIndex = 0;
            _sessionStartTime = Time.time;
            Debug.Log("Command history cleared");
        }

        /// <summary>
        /// Reset all command classes and counters for a fresh replay   
        /// </summary>
        public void ResetAllCommands()
        {
            // Reset unified brawler ID counter
            BrawlerCommandHelper.ResetBrawlerIDCounter();
            
            // Reset model ID counter
            UnityModelBaseClass.ResetModelIDCounter();
            
            // Reset random generator with same seed
            Random = new System.Random(RandomSeed);
            
            Debug.Log("All command counters and ModelID reset for replay");
        }

        /// <summary>
        /// Get current replay progress (0-1)
        /// </summary>
        public float GetReplayProgress()
        {
            if (_commandHistory.Count == 0) return 0f;
            return (float)_replayIndex / _commandHistory.Count;
        }

        /// <summary>
        /// Get current replay time
        /// </summary>
        public float GetCurrentReplayTime()
        {
            return Time.time - _sessionStartTime;
        }
    }
}
