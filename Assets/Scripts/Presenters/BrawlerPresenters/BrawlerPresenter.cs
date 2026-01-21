using Assets.Scripts.Models;
using Assets.Scripts.Healthbars;
using System.ComponentModel;
using Assets.Scripts.Commands;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Strategies;

namespace Assets.Scripts.Presenters
{
    // --- ENUMS FOR INSPECTOR SELECTION ---
    public enum MovementStrategyType
    {
        PlayerInput,        // WASD / Controller
        FollowTank,         // AI: Follows the Player
        Rotational,         // AI: Spins in place
        Stationary          // AI: Does nothing
    }

    public enum AttackExecutionType
    {
        PlayerInput,        // Spacebar / Button (Uses InputSystemAttackStrategy)
        Automated,          // AI: Auto-fire (Uses AutomatedAttackStrategy)
        Dash,               // Dash attack (ElPrimo only)
        None                // Peaceful
    }

    public enum DamageStrategyType
    {
        Standard,           // Constant damage
        DistanceBased,      // Damage falls off over range
        Critical            // Double damage at close range
    }

    public class BrawlerPresenter : PresenterBaseClass<Brawler>
    {
        [Header("Strategy Configuration")]
        [Tooltip("How this Brawler moves.")]
        [SerializeField] private MovementStrategyType movementType = MovementStrategyType.Stationary;

        [Tooltip("How this Brawler decides WHEN to attack.")]
        [SerializeField] private AttackExecutionType attackType = AttackExecutionType.None;

        [Tooltip("How this Brawler calculates damage.")]
        [SerializeField] private DamageStrategyType damageType = DamageStrategyType.Standard;

        [Header("Movement Settings")]
        [SerializeField] protected float moveSpeed = 5f;
        [SerializeField] protected float rotationSpeed = 10f;

        [Header("Attack Settings")]
        [SerializeField] protected float attackInterval = 0.5f;

        [Header("Health Bar Settings")]
        [SerializeField] private bool useCanvasHealthBar = true; // NEW: Toggle between implementations
        [SerializeField] private CanvasHealthBarPresenter canvasHealthBar; // NEW: Canvas-based healthbar
        
        // OLD: UIDocument-based healthbar (deprecated)
        [SerializeField] private VisualTreeAsset healthBarTemplate;
        [SerializeField] private UIDocument _hudDOcument;
        [SerializeField] private Transform healthBarAnchor;

        private Healthbars.HealthBarPresenter _healthBarPresenter;
        private IMovementStrategy _movementStrategy;
        private IAttackStrategy _attack_strategy;
        private PlayerInput _playerInput;
        private bool _subscribedToDied = false;

        //Recording fields
        private float _recordTimer = 0f;
        private const float RECORD_INTERVAL = 0.05f;
        private Vector3 _lastRecordedPos;
        private Quaternion _lastRecordedRot;

        public float MoveSpeed => moveSpeed;
        public float RotationSpeed => rotationSpeed;
        public float AttackInterval => attackInterval;

        public bool IsLocalPlayer => movementType == MovementStrategyType.PlayerInput;
        public AttackExecutionType AttackType => attackType;

        protected override void Awake()
        {
            base.Awake();
            if (_playerInput == null)
            {
                _playerInput = GetComponent<PlayerInput>();
            }
        }

        protected override void ModelSetInitialization(Brawler previousModel)
        {
            base.ModelSetInitialization(previousModel);
            if (previousModel != null)
            {
                previousModel.Died -= OnBrawlerDied;
                _subscribedToDied = false;
                if (previousModel is IHUD prevHud)
                {
                    var hud = HUDModel.Instance;
                    if (hud != null)
                    {
                        if (hud.Slot1 == prevHud) hud.ClearSlot(1);
                        else if (hud.Slot2 == prevHud) hud.ClearSlot(2);
                        else if (hud.Slot3 == prevHud) hud.ClearSlot(3);
                    }
                }
            }
            if (Model != null && !_subscribedToDied)
            {
                Model.Died += OnBrawlerDied;
                _subscribedToDied = true;
            }
        }

        protected void Start()
        {
            // NEW: Choose which healthbar implementation to use
            if (useCanvasHealthBar)
            {
                InitializeCanvasHealthBar();
            }
            else
            {
                ADDHB(_hudDOcument, healthBarTemplate);
            }
            
            InitializeStrategies();
        }

        // NEW: Initialize Canvas-based healthbar
        private void InitializeCanvasHealthBar()
        {
            if (canvasHealthBar == null)
            {
                Debug.LogWarning($"[{name}] Canvas HealthBar is not assigned! Searching in children...");
                canvasHealthBar = GetComponentInChildren<CanvasHealthBarPresenter>();
            }

            if (canvasHealthBar != null && Model != null)
            {
                canvasHealthBar.Configure(Model);
                Debug.Log($"[{name}] Canvas health bar successfully configured");
            }
            else
            {
                Debug.LogError($"[{name}] Failed to initialize Canvas HealthBar!");
            }
        }

        public void ForceInitializeStrategies()
        {
            InitializeStrategies();
        }

        // --- MAIN INITIALIZATION LOGIC ---
        protected virtual void InitializeStrategies()
        {
            // 1. MOVEMENT STRATEGY
            switch (movementType)
            {
                case MovementStrategyType.PlayerInput:
                    if (_playerInput != null && _playerInput.enabled)
                        SetMovementStrategy(new InputSystemMovementStrategy(_playerInput));
                    else
                    {
                        Debug.LogWarning($"[{name}] PlayerInput missing! Defaulting to Stationary.");
                        SetMovementStrategy(new NoMovementStrategy());
                    }
                    break;

                case MovementStrategyType.FollowTank:
                    InitializeFollowStrategy();
                    break;

                case MovementStrategyType.Rotational:
                    SetMovementStrategy(new RotationalMovementStrategy(rotationSpeed * 20f));
                    break;

                case MovementStrategyType.Stationary:
                default:
                    SetMovementStrategy(new NoMovementStrategy());
                    break;
            }

            // 2. ATTACK EXECUTION STRATEGY
            switch (attackType)
            {
                case AttackExecutionType.PlayerInput:
                    if (_playerInput != null && _playerInput.enabled)
                    {
                        SetAttackStrategy(new InputSystemAttackStrategy(_playerInput, attackInterval));
                    }
                    else
                    {
                        Debug.LogWarning($"[{name}] Attack set to PlayerInput but Input is missing.");
                        SetAttackStrategy(null);
                    }
                    break;

                case AttackExecutionType.Automated:
                    SetAttackStrategy(new AutomatedAttackStrategy(attackInterval));
                    break;

                case AttackExecutionType.Dash:
                    // Dash attack is handled by ElPrimopresenter override
                    // Base implementation does nothing
                    Debug.Log($"[{name}] Dash attack type selected (requires ElPrimo)");
                    SetAttackStrategy(null);
                    break;

                case AttackExecutionType.None:
                default:
                    SetAttackStrategy(null);
                    break;
            }

            // 3. DAMAGE STRATEGY
            IDamageStrategy damageStrategy = null;
            switch (damageType)
            {
                case DamageStrategyType.Standard:
                    damageStrategy = new StandardDamageStrategy();
                    break;
                case DamageStrategyType.DistanceBased:
                    damageStrategy = new DistanceBasedDamageStrategy(10f, 0.5f);
                    break;
                case DamageStrategyType.Critical:
                    damageStrategy = new CriticalDamageStrategy(2.0f, 2.0f);
                    break;
            }

            if (Model != null && damageStrategy != null)
            {
                Model.SetDamageStrategy(damageStrategy);
            }

            Debug.Log($"[{gameObject.name}] Strategies Set: Move={movementType}, Attack={attackType}, Damage={damageType}");
        }

        private void InitializeFollowStrategy()
        {
            GameObject[] potentialPlayers = GameObject.FindGameObjectsWithTag("Player");
            Transform targetTank = null;

            foreach (var obj in potentialPlayers)
            {
                var input = obj.GetComponent<PlayerInput>();
                // Find a player that has input and is NOT me
                if (obj != gameObject && input != null && input.enabled)
                {
                    targetTank = obj.transform;
                    break;
                }
            }

            if (targetTank != null)
                SetMovementStrategy(new FollowTheTankStrategy(targetTank));
            else
            {
                SetMovementStrategy(new NoMovementStrategy());
            }
        }

        public void SetMovementStrategy(IMovementStrategy strategy) => _movementStrategy = strategy;
        public void SetAttackStrategy(IAttackStrategy strategy) => _attack_strategy = strategy;

        public void SetPlayerInput(PlayerInput playerInput)
        {
            _playerInput = playerInput;
            // Force this brawler to become the player controller
            movementType = MovementStrategyType.PlayerInput;
            attackType = AttackExecutionType.PlayerInput;
            InitializeStrategies();
        }

        // OLD: UIDocument-based healthbar initialization (kept for backward compatibility)
        public void ADDHB(UIDocument hudDocument, VisualTreeAsset HBxml)
        {
            if (hudDocument == null)
            {
                Debug.LogWarning($"[{name}] UIDocument is not assigned! Health bar will not be created. Please assign '_hudDOcument' in Inspector.");
                return;
            }

            if (HBxml == null)
            {
                Debug.LogWarning($"[{name}] HealthBarTemplate is not assigned! Health bar will not be created. Please assign 'healthBarTemplate' in Inspector.");
                return;
            }

            if (healthBarAnchor == null)
            {
                Debug.LogWarning($"[{name}] HealthBarAnchor is not assigned! Health bar will not be created. Please assign 'healthBarAnchor' in Inspector.");
                return;
            }

            if (Model == null)
            {
                Debug.LogWarning($"[{name}] Model is null! Health bar cannot be created without a model.");
                return;
            }

            _hudDOcument = hudDocument;
            VisualElement cloneRoot = HBxml.CloneTree();
            _healthBarPresenter = new Healthbars.HealthBarPresenter(Model, healthBarAnchor, cloneRoot, _hudDOcument);
            
            Debug.Log($"[{name}] Health bar successfully created");
        }

        private void OnBrawlerDied(object sender, System.EventArgs e)
        {
            Debug.Log($"{Model?.GetType().Name} died. Destroying {gameObject.name}");
            Destroy(gameObject);
        }

        protected override void OnDestroy()
        {
            if (Model != null && _subscribedToDied)
            {
                Model.Died -= OnBrawlerDied;
                _subscribedToDied = false;
            }
            if (_movementStrategy is InputSystemMovementStrategy inputMovement) inputMovement.Cleanup();
            if (_attack_strategy is InputSystemAttackStrategy inputAttack) inputAttack.Cleanup();
            base.OnDestroy();
        }

        protected override void Update()
        {
            base.Update();
            HandleMovement();
            HandleAttack();
            RecordReplayMovement();
        }

        private void RecordReplayMovement()
        {
            var gamePresenter = PD3StartGamePresenter.Instance;
            if (gamePresenter == null) return;

            var history = gamePresenter.CommandHistory;
            if (history == null || history.IsReplaying) return;

            _recordTimer += Time.deltaTime;

            if (_recordTimer >= RECORD_INTERVAL)
            {
                bool hasMoved = Vector3.Distance(transform.position, _lastRecordedPos) > 0.01f;
                bool hasRotated = Quaternion.Angle(transform.rotation, _lastRecordedRot) > 1.0f;

                if (hasMoved || hasRotated)
                {
                    var moveCmd = new BrawlerMoveCommand(Model.ModelID, transform.position, transform.rotation);
                    history.ExecuteCommand(moveCmd);

                    _lastRecordedPos = transform.position;
                    _lastRecordedRot = transform.rotation;
                    _recordTimer = 0f;
                }
            }
        }

        private void LateUpdate()
        {
            // Only update UIDocument healthbar position if using old system
            if (!useCanvasHealthBar)
            {
                _healthBarPresenter?.UpdatePosition();
            }
        }

        protected virtual void HandleMovement()
        {
            _movementStrategy?.Execute(transform, moveSpeed, rotationSpeed, Time.deltaTime);
        }

        protected virtual void HandleAttack()
        {
            // Update cooldown through base class interface
            if (_attack_strategy is AttackStrategyBase attackStrategy)
            {
                attackStrategy.UpdateCooldown(Time.deltaTime);

                if (Model != null)
                {
                    Model.PAProgress = attackStrategy.PAProgress;
                }
            }

            // Execute attack if ready
            if (_attack_strategy != null && _attack_strategy.CanExecute())
            {
                Model?.PARequested();
                _attack_strategy.Execute(Time.deltaTime);
            }
        }

        protected override void Model_PropertyChanged(object sender, PropertyChangedEventArgs e) { }
    }
}