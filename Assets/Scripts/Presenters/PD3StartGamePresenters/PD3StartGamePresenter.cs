using Assets.Scripts.Models;
using Assets.Scripts.Commands;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Assets.Scripts.Interfaces;

namespace Assets.Scripts.Presenters
{
    public class PD3StartGamePresenter : Singleton<PD3StartGamePresenter>, ICommandRegistry
    {
        [Header("Brawler Prefabs")]
        [SerializeField] private GameObject coltPrefab;
        [SerializeField] private GameObject elPrimoPrefab;

        [Header("Spawn Points")]
        [SerializeField] private Transform[] spawnPoints;

        [Header("Replay Settings")]
        [SerializeField] private bool enableReplay = true;
        [SerializeField] private KeyCode replayKey = KeyCode.R;

        private PD3StarsGame _model;
        private CommandHistory _commandHistory;
        private readonly List<GameObject> _brawlerInstances = new List<GameObject>();
        private readonly Dictionary<int, GameObject> _modelIDToGameObject = new Dictionary<int, GameObject>();
        private readonly Dictionary<int, ICommand> _modelIDToCommand = new Dictionary<int, ICommand>();
        private int _nextSpawnIndex = 0;

        // Track the local player that exists in scene (not spawned by commands)
        private GameObject _existingLocalPlayer;
        private Brawler _existingLocalPlayerModel;

        public PD3StarsGame Model => _model;
        public CommandHistory CommandHistory => _commandHistory;

        public enum BrawlerType
        {
            Colt,
            ElPrimo
        }

        protected override void Awake()
        {
            base.Awake();

            // CRITICAL: Register this as the command registry via the locator
            // This allows commands to find us without circular assembly references
            CommandRegistryLocator.Registry = this;

            // Create game model
            _model = new PD3StarsGame();

            // Create command history with optional seed
            _commandHistory = new CommandHistory();

            // Subscribe to model events
            _model.BrawlerAdded += OnBrawlerAdded;
            _model.BrawlerRemoved += OnBrawlerRemoved;

            // Register existing local player in scene FIRST
            RegisterExistingLocalPlayer();

            // Spawn initial brawlers using commands (NPCs only now)
            SpawnInitialBrawlersWithCommands();
        }

        /// <summary>
        /// ICommandRegistry implementation - registers a command for a model ID.
        /// </summary>
        public void RegisterCommandForModel(int modelID, ICommand command)
        {
            _modelIDToCommand[modelID] = command;
            Debug.Log($"[RegisterCommandForModel] Pre-registered command for ModelID {modelID}");
        }

        private void RegisterExistingLocalPlayer()  
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            
            if (playerObject != null)
            {
                Debug.Log($"PD3StartGamePresenter: Found existing player: {playerObject.name}");
                
                var presenter = playerObject.GetComponent<BrawlerPresenter>();
                if (presenter != null && presenter.Model != null)
                {
                    _existingLocalPlayer = playerObject;
                    _existingLocalPlayerModel = presenter.Model;

                    // Register as local player WITHOUT triggering CreatePresenterForBrawler
                    // but DO trigger HUD registration
                    _model.BrawlerAdded -= OnBrawlerAdded;
                    _model.AddBrawler(presenter.Model, isLocalPlayer: true);
                    
                    // Manually register with HUD since we skipped OnBrawlerAdded
                    var hudModel = HUDModel.Instance;
                    if (hudModel != null && presenter.Model is IHUD hudThing)
                    {
                        hudModel.AssignNext(hudThing);
                        Debug.Log($"PD3StartGamePresenter: Registered existing local player with HUD");
                    }
                    
                    _model.BrawlerAdded += OnBrawlerAdded;

                    _brawlerInstances.Add(playerObject);
                    _modelIDToGameObject[presenter.Model.ModelID] = playerObject;
                    
                    Debug.Log($"PD3StartGamePresenter: Registered existing local player: {presenter.Model.GetType().Name}, ModelID={presenter.Model.ModelID}");
                }
                else
                {
                    Debug.LogError($"PD3StartGamePresenter: Player GameObject '{playerObject.name}' has no BrawlerPresenter or Model!");
                }
            }
            else
            {
                Debug.LogWarning("PD3StartGamePresenter: No existing GameObject with 'Player' tag found in scene!");
            }
        }

        private void SpawnInitialBrawlersWithCommands()
        {
            // Only spawn NPC brawlers now (local player already exists)
            AddBrawlerWithCommand(BrawlerType.ElPrimo, isLocalPlayer: false);
            AddBrawlerWithCommand(BrawlerType.Colt, isLocalPlayer: false);
        }

        private Transform GetNextSpawnPoint()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogWarning("No spawn points assigned! Using default position.");
                return null;
            }

            Transform spawnPoint = spawnPoints[_nextSpawnIndex % spawnPoints.Length];
            _nextSpawnIndex++;
            Debug.Log($"[GetNextSpawnPoint] Returning spawn point index {_nextSpawnIndex - 1}: {spawnPoint.position}");
            return spawnPoint;
        }

        /// <summary>
        /// Add brawler using Command Pattern for replay support
        /// </summary>
        public void AddBrawlerWithCommand(BrawlerType brawlerType, bool isLocalPlayer = false)
        {
            // Get spawn position
            Transform spawnPoint = GetNextSpawnPoint();
            Vector3 position = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            Quaternion rotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

            Debug.Log($"[AddBrawlerWithCommand] Spawning {brawlerType} at spawn point position: {position}, rotation: {rotation.eulerAngles}");

            // Create appropriate command
            ICommand command = brawlerType switch
            {
                BrawlerType.Colt => new CreateColtCommand(_model, position, rotation, isLocalPlayer),
                BrawlerType.ElPrimo => new CreateElPrimoCommand(_model, position, rotation, isLocalPlayer),
                _ => null
            };

            if (command != null && enableReplay)
            {
                // Execute through command history for replay support
                _commandHistory.ExecuteCommand(command);
            }
            else if (command != null)
            {
                // Direct execution without replay
                command.Execute();
            }
        }

        public void AddBrawler(BrawlerType brawlerType, bool isLocalPlayer = false)
        {
            GameObject prefab = brawlerType switch
            {
                BrawlerType.Colt => coltPrefab,
                BrawlerType.ElPrimo => elPrimoPrefab,
                _ => null
            };

            if (prefab == null)
            {
                Debug.LogError($"Prefab for {brawlerType} not assigned!");
                return;
            }

            // Get spawn position
            Transform spawnPoint = GetNextSpawnPoint();
            Vector3 position = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            Quaternion rotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

            // Instantiate
            GameObject instance = Instantiate(prefab, position, rotation);
            
            // Configure based on local/NPC
            ConfigureBrawlerInstance(instance, isLocalPlayer);

            // Get presenter and add model to game
            var presenter = instance.GetComponent<BrawlerPresenter>();
            if (presenter != null && presenter.Model != null)
            {
                _model.AddBrawler(presenter.Model, isLocalPlayer);
                _modelIDToGameObject[presenter.Model.ModelID] = instance;
            }

            _brawlerInstances.Add(instance);
            Debug.Log($"Spawned {brawlerType} as {(isLocalPlayer ? "Local Player" : "NPC")}");
        }

        private void ConfigureBrawlerInstance(GameObject instance, bool isLocalPlayer)
        {
            if (isLocalPlayer)
            {
                instance.tag = "Player";
                var playerInput = instance.GetComponent<PlayerInput>();
                if (playerInput != null)
                {
                    playerInput.enabled = true;
                    playerInput.ActivateInput();
                }
            }
            else
            {
                instance.tag = "Untagged";
                var playerInput = instance.GetComponent<PlayerInput>();
                if (playerInput != null)
                {
                    playerInput.enabled = false;
                }
            }

            var presenter = instance.GetComponent<BrawlerPresenter>();
            presenter?.ForceInitializeStrategies();
        }

        private void OnBrawlerAdded(object sender, BrawlerAddedEventArgs e)
        {
            Debug.Log($"[OnBrawlerAdded] Brawler added to game: {e.Brawler.GetType().Name}, ModelID={e.Brawler.ModelID}, IsLocal: {e.IsLocalPlayer}");

            // Create presenter GameObject for the model (all NPCs come through commands now)
            CreatePresenterForBrawler(e.Brawler, e.IsLocalPlayer);

            // Assign brawlers to HUDModel slots
            var hudModel = HUDModel.Instance;
            if (hudModel != null && e.Brawler is IHUD hudThing)
            {
                hudModel.AssignNext(hudThing);
            }
        }

        private void CreatePresenterForBrawler(Brawler brawler, bool isLocalPlayer)
        {
            GameObject prefab = brawler switch
            {
                Colt => coltPrefab,
                ElPrimo => elPrimoPrefab,
                _ => null
            };

            if (prefab == null)
            {
                Debug.LogError($"Prefab for {brawler.GetType().Name} not assigned!");
                return;
            }

            // Get spawn position from the command that created this brawler
            Vector3 position = Vector3.zero;
            Quaternion rotation = Quaternion.identity;
            bool foundCommand = false;

            if (_modelIDToCommand.TryGetValue(brawler.ModelID, out ICommand command))
            {
                if (command is CreateColtCommand coltCmd)
                {
                    position = coltCmd.SpawnPosition;
                    rotation = coltCmd.SpawnRotation;
                    foundCommand = true;
                    Debug.Log($"[CreatePresenterForBrawler] Found Colt command for ModelID {brawler.ModelID} with spawn position: {position}");
                }
                else if (command is CreateElPrimoCommand primoCmd)
                {
                    position = primoCmd.SpawnPosition;
                    rotation = primoCmd.SpawnRotation;
                    foundCommand = true;
                    Debug.Log($"[CreatePresenterForBrawler] Found ElPrimo command for ModelID {brawler.ModelID} with spawn position: {position}");
                }
            }

            if (!foundCommand)
            {
                Debug.LogWarning($"[CreatePresenterForBrawler] No command found for ModelID {brawler.ModelID}, using default position (0,0,0)");
            }

            // Instantiate prefab at the correct position
            GameObject instance = Instantiate(prefab, position, rotation);
            
            Debug.Log($"[CreatePresenterForBrawler] Instantiated {brawler.GetType().Name} prefab at position: {instance.transform.position}");

            // Configure based on local/NPC
            ConfigureBrawlerInstance(instance, isLocalPlayer);

            // Get presenter and link to model BEFORE any Start() calls
            var presenter = instance.GetComponent<BrawlerPresenter>();
            if (presenter != null)
            {
                // Set the model IMMEDIATELY to prevent Awake from creating a new one
                presenter.Model = brawler;
                Debug.Log($"[CreatePresenterForBrawler] Linked presenter to model: ModelID={brawler.ModelID}, GameObject position: {instance.transform.position}");
            }
            else
            {
                Debug.LogError($"[CreatePresenterForBrawler] No BrawlerPresenter component found on instantiated prefab!");
            }

            _brawlerInstances.Add(instance);
            _modelIDToGameObject[brawler.ModelID] = instance;
            
            Debug.Log($"Created presenter for {brawler.GetType().Name} ModelID={brawler.ModelID} at final position: {instance.transform.position}");
        }

        private void OnBrawlerRemoved(object sender, BrawlerRemovedEventArgs e)
        {
            Debug.Log($"Brawler removed from game: {e.Brawler.GetType().Name}, ModelID={e.Brawler.ModelID}");
            
            // Remove from HUD
            var hudModel = HUDModel.Instance;
            if (hudModel != null && e.Brawler is IHUD hudThing)
            {
                hudModel.Remove(hudThing);
            }

            // Don't destroy the existing local player
            if (_existingLocalPlayer != null && _modelIDToGameObject.TryGetValue(e.Brawler.ModelID, out GameObject instance))
            {
                if (instance == _existingLocalPlayer)
                {
                    Debug.Log("Skipping destruction of existing local player during replay");
                    return;
                }
            }

            // Destroy associated GameObject (but DON'T remove command - needed for replay)
            if (_modelIDToGameObject.TryGetValue(e.Brawler.ModelID, out GameObject instanceToDestroy))
            {
                _brawlerInstances.Remove(instanceToDestroy);
                _modelIDToGameObject.Remove(e.Brawler.ModelID);
                Destroy(instanceToDestroy);
            }
        }

        protected override void OnDestroy()
        {
            if (_model != null)
            {
                _model.BrawlerAdded -= OnBrawlerAdded;
                _model.BrawlerRemoved -= OnBrawlerRemoved;
            }
            
            // Clear static reference from locator
            if (CommandRegistryLocator.Registry == this)
            {
                CommandRegistryLocator.Registry = null;
            }
            
            base.OnDestroy();
        }

        private void Update()
        {
            _model?.Update();

            // Handle replay input
            if (enableReplay && Input.GetKeyDown(replayKey) && !_commandHistory.IsReplaying)
            {
                StartReplay();
            }

            // Update replay
            if (_commandHistory.IsReplaying)
            {
                float replayTime = _commandHistory.GetCurrentReplayTime();
                _commandHistory.ReplayUntil(replayTime);
            }
        }

        /// <summary>
        /// Start replay: clear scene and replay all commands
        /// </summary>
        public void StartReplay()
        {
            Debug.Log("Starting replay...");

            // Clear all NPC brawlers (keep existing local player)
            ClearNPCBrawlers();

            // Reset spawn index
            _nextSpawnIndex = 0;

            // Reset all command counters and ModelID
            _commandHistory.ResetAllCommands();

            // Re-register existing local player WITHOUT triggering event
            if (_existingLocalPlayer != null && _existingLocalPlayerModel != null)
            {
                _model.BrawlerAdded -= OnBrawlerAdded;
                _model.AddBrawler(_existingLocalPlayerModel, isLocalPlayer: true);
                _model.BrawlerAdded += OnBrawlerAdded;
                
                if (!_brawlerInstances.Contains(_existingLocalPlayer))
                {
                    _brawlerInstances.Add(_existingLocalPlayer);
                }
                
                if (!_modelIDToGameObject.ContainsKey(_existingLocalPlayerModel.ModelID))
                {
                    _modelIDToGameObject[_existingLocalPlayerModel.ModelID] = _existingLocalPlayer;
                }
                
                Debug.Log($"Re-registered existing local player for replay: ModelID={_existingLocalPlayerModel.ModelID}");
            }

            // Start replay mode
            _commandHistory.StartReplay();
        }

        private void ClearNPCBrawlers()
        {
            // IMPORTANT: Get the list BEFORE modifying anything
            // Make a copy of the brawlers list to avoid modification during iteration
            var allBrawlers = _model.Brawlers.ToList();
            
            Debug.Log($"[ClearNPCBrawlers] Total brawlers in model: {allBrawlers.Count}");

            // Get list of brawlers to remove (all except existing local player)
            var brawlersToRemove = allBrawlers
                .Where(b => _existingLocalPlayerModel == null || b.ModelID != _existingLocalPlayerModel.ModelID)
                .ToList();

            Debug.Log($"[ClearNPCBrawlers] Brawlers to remove: {brawlersToRemove.Count}");

            // First, remove from HUD explicitly for each NPC
            var hudModel = HUDModel.Instance;
            foreach (var brawler in brawlersToRemove)
            {
                if (hudModel != null && brawler is IHUD hudThing)
                {
                    hudModel.Remove(hudThing);
                    Debug.Log($"[ClearNPCBrawlers] Removed HUD for {brawler.GetType().Name} ModelID={brawler.ModelID}");
                }
            }

            // Destroy NPC GameObjects
            var instancesToRemove = _brawlerInstances.Where(i => i != null && i != _existingLocalPlayer).ToList();
            foreach (var instance in instancesToRemove)
            {
                Debug.Log($"[ClearNPCBrawlers] Destroying GameObject: {instance.name}");
                _brawlerInstances.Remove(instance);
                Destroy(instance);
            }

            // Clear GameObject mappings for NPCs only (keep command mappings!)
            var modelIDsToRemove = _modelIDToGameObject
                .Where(kvp => kvp.Value != _existingLocalPlayer)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var modelID in modelIDsToRemove)
            {
                _modelIDToGameObject.Remove(modelID);
            }

            // Remove NPC brawlers from model (this will also trigger OnBrawlerRemoved but GameObjects already destroyed)
            foreach (var brawler in brawlersToRemove)
            {
                _model.RemoveBrawler(brawler);
            }

            Debug.Log($"Cleared {brawlersToRemove.Count} NPC brawlers for replay (preserved local player)");
        }

        public GameObject GetGameObjectByModelID(int modelID)
        {
            _modelIDToGameObject.TryGetValue(modelID, out GameObject gameObject);
            return gameObject;
        }
    }
}