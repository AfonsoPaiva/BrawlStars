using Assets.Scripts.Models;
using Assets.Scripts.Strategies.Attack;
using Assets.Scripts.Strategies.Movement;
using PD3HealthBars;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Assets.Scripts.Presenters
{
    public class BrawlerPresenter : PresenterBaseClass<Brawler>
    {
        [Header("Movement Settings")]
        [SerializeField] protected float moveSpeed = 5f;
        [SerializeField] protected float rotationSpeed = 10f;

        [Header("Attack Settings")]
        [SerializeField] protected float attackInterval = 0.5f;

        [Header("Health Bar Settings")]
        [SerializeField] private VisualTreeAsset healthBarTemplate;
        [SerializeField] private UIDocument _hudDOcument;
        [SerializeField] private Transform healthBarAnchor;

        private PD3HealthBars.HealthBarPresenter _healthBarPresenter;
        private IMovementStrategy _movementStrategy;
        private IAttackStrategy _attack_strategy;
        private PlayerInput _playerInput;

        public float MoveSpeed => moveSpeed;
        public float RotationSpeed => rotationSpeed;
        public float AttackInterval => attackInterval;

        // Property to check if this is the local player based on tag
        public bool IsLocalPlayer
        {
            get => gameObject.CompareTag("Player");
        }

        protected override void Awake()
        {
            base.Awake();

            // Try to get PlayerInput component on awake
            if (_playerInput == null)
            {
                _playerInput = GetComponent<PlayerInput>();
            }
        }

        protected override void ModelSetInitialization(Brawler previousModel)
        {
            base.ModelSetInitialization(previousModel);

            // Unsubscribe from previous model
            if (previousModel != null)
            {
                previousModel.Died -= OnBrawlerDied;
            }

            // Subscribe to new model
            if (Model != null)
            {
                Model.Died += OnBrawlerDied;
            }
        }

        protected void Start()
        {
            ADDHB(_hudDOcument, healthBarTemplate);

            // Initialize strategies
            InitializeStrategies();

            // Subscribe to death event if model is already set
            if (Model != null)
            {
                Model.Died += OnBrawlerDied;
            }
        }

        // Public method to force initialize strategies (called by game presenter)
        public void ForceInitializeStrategies()
        {
            InitializeStrategies();
        }

        protected virtual void InitializeStrategies()
        {
            if (IsLocalPlayer)
            {
                // For local player: use input-based strategies
                if (_playerInput != null && _playerInput.enabled)
                {
                    SetMovementStrategy(new InputSystemMovementStrategy(_playerInput));
                    SetAttackStrategy(new InputSystemAttackStrategy(_playerInput, attackInterval));
                    Debug.Log($"Initialized local player strategies for {gameObject.name}");
                }
                else
                {
                    Debug.LogWarning($"{gameObject.name}: Object has Player tag but PlayerInput is missing or disabled. Using non-local strategies.");
                    SetMovementStrategy(CreateNonLocalMovementStrategy());
                    SetAttackStrategy(CreateNonLocalAttackStrategy());
                }
            }
            else
            {
                // For NPCs: use automated strategies
                SetMovementStrategy(CreateNonLocalMovementStrategy());
                SetAttackStrategy(CreateNonLocalAttackStrategy());
                Debug.Log($"Initialized NPC strategies for {gameObject.name}");
            }
        }

        protected virtual IMovementStrategy CreateNonLocalMovementStrategy()
        {
            return new NoMovementStrategy();
        }

        protected virtual IAttackStrategy CreateNonLocalAttackStrategy()
        {
            return null;
        }

        public void SetMovementStrategy(IMovementStrategy strategy)
        {
            _movementStrategy = strategy;
        }

        public void SetAttackStrategy(IAttackStrategy strategy)
        {
            _attack_strategy = strategy;
        }

        // Method to set PlayerInput externally (if needed)
        public void SetPlayerInput(PlayerInput playerInput)
        {
            _playerInput = playerInput;
            InitializeStrategies();
        }

        public void ADDHB(UIDocument hudDocument, VisualTreeAsset HBxml)
        {
            _hudDOcument = hudDocument;
            VisualElement cloneRoot = HBxml.CloneTree();
            Transform hbTransform = healthBarAnchor;
            if (hbTransform != null)
            {
                _healthBarPresenter = new PD3HealthBars.HealthBarPresenter(Model, hbTransform, cloneRoot, _hudDOcument);
            }
        }

        private void OnBrawlerDied(object sender, System.EventArgs e)
        {
            Debug.Log($"{Model.GetType().Name} died! Destroying GameObject.");
            Destroy(gameObject);
        }

        protected override void OnDestroy()
        {
            // Unsubscribe from model events
            if (Model != null)
            {
                Model.Died -= OnBrawlerDied;
            }

            // Cleanup strategy subscriptions
            if (_movementStrategy is InputSystemMovementStrategy inputMovement)
            {
                inputMovement.Cleanup();
            }

            if (_attack_strategy is InputSystemAttackStrategy inputAttack)
            {
                inputAttack.Cleanup();
            }

            base.OnDestroy();
        }

        protected override void Update()
        {
            base.Update();
            HandleMovement();
            HandleAttack();
        }

        private void LateUpdate()
        {
            _healthBarPresenter?.UpdatePosition();
        }

        protected virtual void HandleMovement()
        {
            _movementStrategy?.Execute(transform, moveSpeed, rotationSpeed, Time.deltaTime);
        }

        protected virtual void HandleAttack()
        {
            // Update cooldown for strategies that need it every frame
            if (_attack_strategy is AutomatedAttackStrategy automatedStrategy)
            {
                automatedStrategy.UpdateCooldown(Time.deltaTime);
            }
            else if (_attack_strategy is InputSystemAttackStrategy inputAttackStrategy)
            {
                inputAttackStrategy.UpdateCooldown(Time.deltaTime);
            }

            if (_attack_strategy != null && _attack_strategy.CanExecute())
            {
                Model?.PARequested();
                _attack_strategy.Execute(Time.deltaTime);
            }
        }

        protected override void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }
    }
}