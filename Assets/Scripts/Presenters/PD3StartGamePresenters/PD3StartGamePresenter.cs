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
        [SerializeField] private float returnToSpawnSpeed = 10f; // Speed for animating back to spawn

        private PD3StarsGame _model;
        private CommandHistory _commandHistory;
        private readonly List<GameObject> _brawlerInstances = new List<GameObject>();
        private readonly Dictionary<int, GameObject> _modelIDToGameObject = new Dictionary<int, GameObject>();
        private readonly Dictionary<int, ICommand> _modelIDToCommand = new Dictionary<int, ICommand>();
        private int _nextSpawnIndex = 0;
        private int _initialSpawnIndex = 0;

        // Track Player Start
        private Vector3 _localPlayerStartPos;
        private Quaternion _localPlayerStartRot;

        // Track the local player that exists in scene
        private GameObject _existingLocalPlayer;
        private Brawler _existingLocalPlayerModel;

        // Track initial spawn positions for replay
        private readonly Dictionary<int, Vector3> _initialBrawlerPositions = new Dictionary<int, Vector3>();
        private readonly Dictionary<int, Quaternion> _initialBrawlerRotations = new Dictionary<int, Quaternion>();

        // NEW: Replay animation state
        private bool _isAnimatingToSpawn = false;
        private readonly Dictionary<int, Vector3Animation> _returnToSpawnAnimations = new Dictionary<int, Vector3Animation>();

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

            // Validate prefabs and spawn points
            ValidateConfiguration();

            CommandRegistryLocator.Registry = this;

            _model = new PD3StarsGame();
            _commandHistory = new CommandHistory();

            BrawlerMoveCommand.FindGameObjectByID = this.GetGameObjectByModelID;

            _model.BrawlerAdded += OnBrawlerAdded;
            _model.BrawlerRemoved += OnBrawlerRemoved;

            RegisterExistingLocalPlayer();
            
            // CRITICAL FIX: Synchronize BrawlerCommandHelper counter with existing player's ModelID
            // This ensures ElPrimo gets ModelID=2, not ModelID=1 (which is already taken)
            if (_existingLocalPlayerModel != null)
            {
                // Advance the BrawlerCommandHelper counter to match the next available ID
                while (BrawlerCommandHelper.GetNextBrawlerID() <= _existingLocalPlayerModel.ModelID)
                {
                    // Keep incrementing until we're past the existing player's ID
                }
                Debug.Log($"[PD3StartGamePresenter] Synchronized BrawlerCommandHelper counter past existing player's ModelID ({_existingLocalPlayerModel.ModelID})");
            }
            
            // FIXED: Set initial spawn index AFTER registering local player
            _initialSpawnIndex = _nextSpawnIndex;
            Debug.Log($"[PD3StartGamePresenter] Initial spawn index set to {_initialSpawnIndex}, next spawn will use index {_nextSpawnIndex}");

            SpawnInitialBrawlersWithCommands();
        }

        /// <summary>
        /// Validate that all required prefabs and spawn points are assigned
        /// </summary>
        private void ValidateConfiguration()
        {
            if (coltPrefab == null)
            {
                Debug.LogError("[PD3StartGamePresenter] Colt prefab is not assigned! Please assign it in the Inspector.");
            }

            if (elPrimoPrefab == null)
            {
                Debug.LogError("[PD3StartGamePresenter] ElPrimo prefab is not assigned! Please assign it in the Inspector.");
            }

            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogError("[PD3StartGamePresenter] No spawn points assigned! Brawlers will spawn at (0,0,0).");
            }
            else
            {
                Debug.Log($"[PD3StartGamePresenter] Configuration valid: {spawnPoints.Length} spawn points available");
                for (int i = 0; i < spawnPoints.Length; i++)
                {
                    if (spawnPoints[i] != null)
                    {
                        Debug.Log($"[PD3StartGamePresenter] Spawn Point {i}: {spawnPoints[i].name} at {spawnPoints[i].position}");
                    }
                    else
                    {
                        Debug.LogWarning($"[PD3StartGamePresenter] Spawn Point {i} is null!");
                    }
                }
            }
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
                Debug.Log($"[PD3StartGamePresenter] Found existing player: {playerObject.name}");

                var presenter = playerObject.GetComponent<BrawlerPresenter>();
                if (presenter != null && presenter.Model != null)
                {
                    _existingLocalPlayer = playerObject;
                    _existingLocalPlayerModel = presenter.Model;

                    _localPlayerStartPos = playerObject.transform.position;
                    _localPlayerStartRot = playerObject.transform.rotation;

                    Debug.Log($"[PD3StartGamePresenter] Recorded Local Player Start: Position={_localPlayerStartPos}, Rotation={_localPlayerStartRot.eulerAngles}");
                    Debug.Log($"[PD3StartGamePresenter] Existing player has ModelID={_existingLocalPlayerModel.ModelID}");

                    // CRITICAL FIX: Temporarily unsubscribe to prevent OnBrawlerAdded from creating duplicate
                    _model.BrawlerAdded -= OnBrawlerAdded;
                    _model.AddBrawler(presenter.Model, isLocalPlayer: true);
                    _model.BrawlerAdded += OnBrawlerAdded;

                    // Manually register with HUD since we skipped OnBrawlerAdded
                    var hudModel = HUDModel.Instance;
                    if (hudModel != null && presenter.Model is IHUD hudThing)
                    {
                        hudModel.AssignNext(hudThing);
                        Debug.Log($"[PD3StartGamePresenter] Registered existing local player with HUD");
                    }

                    _brawlerInstances.Add(playerObject);
                    _modelIDToGameObject[presenter.Model.ModelID] = playerObject;

                    // Store initial position for replay
                    _initialBrawlerPositions[presenter.Model.ModelID] = _localPlayerStartPos;
                    _initialBrawlerRotations[presenter.Model.ModelID] = _localPlayerStartRot;

                    // FIXED: Increment spawn index to reserve this position
                    _nextSpawnIndex++;

                    Debug.Log($"[PD3StartGamePresenter] Registered existing local player: {presenter.Model.GetType().Name}, ModelID={presenter.Model.ModelID}, Next spawn index: {_nextSpawnIndex}");
                }
                else
                {
                    Debug.LogError($"[PD3StartGamePresenter] Player GameObject '{playerObject.name}' has no BrawlerPresenter or Model!");
                }
            }
            else
            {
                Debug.LogWarning("[PD3StartGamePresenter] No existing GameObject with 'Player' tag found in scene!");
            }
        }

        private void SpawnInitialBrawlersWithCommands()
        {
            Debug.Log($"[PD3StartGamePresenter] === Starting Initial Brawler Spawns ===");
            Debug.Log($"[PD3StartGamePresenter] Current spawn index: {_nextSpawnIndex}");

            // Only spawn NPC brawlers now (local player already exists)
            AddBrawlerWithCommand(BrawlerType.ElPrimo, isLocalPlayer: false);
            AddBrawlerWithCommand(BrawlerType.Colt, isLocalPlayer: false);

            Debug.Log($"[PD3StartGamePresenter] === Initial Brawler Spawns Complete ===");
        }

        private Transform GetNextSpawnPoint()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogWarning("[GetNextSpawnPoint] No spawn points assigned! Using default position.");
                return null;
            }

            Transform spawnPoint = spawnPoints[_nextSpawnIndex % spawnPoints.Length];
            int currentIndex = _nextSpawnIndex;
            _nextSpawnIndex++;

            Debug.Log($"[GetNextSpawnPoint] Using spawn point index {currentIndex}: {spawnPoint?.name ?? "null"} at {spawnPoint?.position ?? Vector3.zero}");

            return spawnPoint;
        }

        /// <summary>
        /// Add brawler using Command Pattern for replay support
        /// </summary>
        public void AddBrawlerWithCommand(BrawlerType brawlerType, bool isLocalPlayer = false)
        {
            Debug.Log($"[AddBrawlerWithCommand] === Creating {brawlerType} (isLocal: {isLocalPlayer}) ===");

            // Get spawn position
            Transform spawnPoint = GetNextSpawnPoint();
            Vector3 position = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            Quaternion rotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

            Debug.Log($"[AddBrawlerWithCommand] Spawn point: {spawnPoint?.name ?? "null"}");
            Debug.Log($"[AddBrawlerWithCommand] Position: {position}, Rotation: {rotation.eulerAngles}");

            // Create appropriate command
            ICommand command = brawlerType switch
            {
                BrawlerType.Colt => new CreateColtCommand(_model, position, rotation, isLocalPlayer),
                BrawlerType.ElPrimo => new CreateElPrimoCommand(_model, position, rotation, isLocalPlayer),
                _ => null
            };

            if (command == null)
            {
                Debug.LogError($"[AddBrawlerWithCommand] Failed to create command for {brawlerType}!");
                return;
            }

            if (enableReplay)
            {
                // Execute through command history for replay support
                Debug.Log($"[AddBrawlerWithCommand] Executing {brawlerType} command through command history");
                _commandHistory.ExecuteCommand(command);
            }
            else
            {
                // Direct execution without replay
                Debug.Log($"[AddBrawlerWithCommand] Executing {brawlerType} command directly (replay disabled)");
                command.Execute();
            }
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
            Debug.Log($"[OnBrawlerAdded] === Brawler Added Event === Type: {e.Brawler.GetType().Name}, ModelID: {e.Brawler.ModelID}, IsLocal: {e.IsLocalPlayer}");

            // CRITICAL FIX: Check if this is the existing local player to avoid duplicate creation
            if (_existingLocalPlayerModel != null && e.Brawler.ModelID == _existingLocalPlayerModel.ModelID)
            {
                Debug.Log($"[OnBrawlerAdded] Skipping creation - this is the existing local player (ModelID={e.Brawler.ModelID})");
                return;
            }

            // Create presenter GameObject for the model (all NPCs come through commands now)
            CreatePresenterForBrawler(e.Brawler, e.IsLocalPlayer);

            // Assign brawlers to HUDModel slots
            var hudModel = HUDModel.Instance;
            if (hudModel != null && e.Brawler is IHUD hudThing)
            {
                hudModel.AssignNext(hudThing);
                Debug.Log($"[OnBrawlerAdded] Assigned {e.Brawler.GetType().Name} to HUD");
            }
        }

        private void CreatePresenterForBrawler(Brawler brawler, bool isLocalPlayer)
        {
            Debug.Log($"[CreatePresenterForBrawler] === Creating Presenter for {brawler.GetType().Name} (ModelID: {brawler.ModelID}) ===");

            GameObject prefab = brawler switch
            {
                Colt => coltPrefab,
                ElPrimo => elPrimoPrefab,
                _ => null
            };

            if (prefab == null)
            {
                Debug.LogError($"[CreatePresenterForBrawler] CRITICAL ERROR: Prefab for {brawler.GetType().Name} not assigned! Check Inspector on PD3StartGamePresenter.");
                return;
            }

            Debug.Log($"[CreatePresenterForBrawler] Using prefab: {prefab.name}");

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

            Debug.Log($"[CreatePresenterForBrawler] Instantiated {brawler.GetType().Name} prefab '{instance.name}' at position: {instance.transform.position}");

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
                Debug.LogError($"[CreatePresenterForBrawler] CRITICAL ERROR: No BrawlerPresenter component found on instantiated prefab '{instance.name}'!");
            }

            _brawlerInstances.Add(instance);
            _modelIDToGameObject[brawler.ModelID] = instance;

            // Store initial position for replay
            _initialBrawlerPositions[brawler.ModelID] = position;
            _initialBrawlerRotations[brawler.ModelID] = rotation;

            Debug.Log($"[CreatePresenterForBrawler] === Successfully created presenter for {brawler.GetType().Name} ModelID={brawler.ModelID} at position: {instance.transform.position} ===");
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

            // Handle "return to spawn" animation
            if (_isAnimatingToSpawn)
            {
                UpdateReturnToSpawnAnimations();
                return; // Don't handle other input during animation
            }

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

                // Check if replay finished
                if (!_commandHistory.IsReplaying)
                {
                    OnReplayFinished();
                }
            }
        }

        /// <summary>
        /// NEW: Animate all brawlers back to spawn positions
        /// </summary>
        private void UpdateReturnToSpawnAnimations()
        {
            bool allFinished = true;

            foreach (var kvp in _returnToSpawnAnimations)
            {
                int modelID = kvp.Key;
                Vector3Animation animation = kvp.Value;

                if (animation.IsPlaying)
                {
                    animation.Update(Time.deltaTime);

                    // Apply animation to GameObject
                    if (_modelIDToGameObject.TryGetValue(modelID, out GameObject brawlerObj))
                    {
                        brawlerObj.transform.position = animation.CurrentUnityValue;
                    }

                    allFinished = false;
                }
            }

            // When all animations finish, start the actual replay
            if (allFinished)
            {
                _isAnimatingToSpawn = false;
                _returnToSpawnAnimations.Clear();

                Debug.Log("=== All brawlers returned to spawn, starting command replay ===");

                // NOW start the actual replay
                BeginCommandReplay();
            }
        }

        /// <summary>
        /// Start replay: animate brawlers back to spawn points
        /// </summary>
        public void StartReplay()
        {
            Debug.Log("=== Starting Replay - Phase 1: Animating to spawn points ===");

            // Disable all input
            DisableAllBrawlerInput();

            // Clear all NPC brawlers
            ClearNPCBrawlers();

            // Reset spawn index
            _nextSpawnIndex = _initialSpawnIndex;

            // Reset all command counters and ModelID
            _commandHistory.ResetAllCommands();
            
            // CRITICAL: Re-synchronize after reset
            if (_existingLocalPlayerModel != null)
            {
                while (BrawlerCommandHelper.GetNextBrawlerID() <= _existingLocalPlayerModel.ModelID)
                {
                    // Keep incrementing until we're past the existing player's ID
                }
                Debug.Log($"[StartReplay] Re-synchronized BrawlerCommandHelper counter past existing player's ModelID ({_existingLocalPlayerModel.ModelID})");
            }

            // Re-register existing local player
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

            // Create animations to return all brawlers to spawn
            _returnToSpawnAnimations.Clear();

            foreach (var kvp in _modelIDToGameObject)
            {
                int modelID = kvp.Key;
                GameObject brawlerObj = kvp.Value;

                if (brawlerObj != null && _initialBrawlerPositions.ContainsKey(modelID))
                {
                    Vector3 currentPos = brawlerObj.transform.position;
                    Vector3 targetPos = _initialBrawlerPositions[modelID];

                    float distance = Vector3.Distance(currentPos, targetPos);
                    float duration = distance / returnToSpawnSpeed;

                    // Create animation for this brawler
                    var animation = new Vector3Animation(currentPos, targetPos, duration);
                    animation.Play();

                    _returnToSpawnAnimations[modelID] = animation;

                    Debug.Log($"[Replay] Creating return animation for ModelID {modelID}: {currentPos} -> {targetPos} (duration: {duration:F2}s)");
                }
            }

            _isAnimatingToSpawn = true;
        }

        /// <summary>
        /// NEW: Begin the actual command replay after animation completes
        /// </summary>
        private void BeginCommandReplay()
        {
            // Set rotations to initial spawn rotations
            foreach (var kvp in _modelIDToGameObject)
            {
                int modelID = kvp.Key;
                GameObject brawlerObj = kvp.Value;

                if (brawlerObj != null && _initialBrawlerRotations.ContainsKey(modelID))
                {
                    brawlerObj.transform.rotation = _initialBrawlerRotations[modelID];
                }
            }

            // Start replay mode
            _commandHistory.StartReplay();

            Debug.Log("=== Replay Phase 2: Executing commands ===");
        }

        /// <summary>
        /// NEW: Called when replay finishes to re-enable input
        /// </summary>
        private void OnReplayFinished()
        {
            Debug.Log("=== Replay Finished - Re-enabling input ===");

            // Re-enable player input
            if (_existingLocalPlayer != null)
            {
                var input = _existingLocalPlayer.GetComponent<PlayerInput>();
                if (input != null)
                {
                    input.enabled = true;
                    input.ActivateInput();
                    Debug.Log("Player input re-enabled");
                }
            }
        }

        /// <summary>
        /// NEW: Disable input for all brawlers
        /// </summary>
        private void DisableAllBrawlerInput()
        {
            foreach (var instance in _brawlerInstances)
            {
                if (instance != null)
                {
                    var input = instance.GetComponent<PlayerInput>();
                    if (input != null)
                    {
                        input.enabled = false;
                        Debug.Log($"Disabled input for {instance.name}");
                    }
                }
            }
        }

        private void ClearNPCBrawlers()
        {
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