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

        [Header("Input")]
        [SerializeField] protected PlayerInput playerInput;

        [Header("Control")]
        [SerializeField] protected bool isLocalPlayer = false;

        [Header("Health Bar Settings")]
        [SerializeField] private VisualTreeAsset healthBarTemplate;
        [SerializeField] private UIDocument _hudDOcument;
        [SerializeField] private Transform healthBarAnchor;

        private PD3HealthBars.HealthBarPresenter _healthBarPresenter;
        private IMovementStrategy _movementStrategy;
        private IAttackStrategy _attackStrategy;

        public float MoveSpeed => moveSpeed;
        public float RotationSpeed => rotationSpeed;
        public bool IsLocalPlayer => isLocalPlayer;

        protected override void Awake() 
        {
            base.Awake();
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
            InitializeStrategies();
            
            // Subscribe to death event if model is already set
            if (Model != null)
            {
                Model.Died += OnBrawlerDied;
            }
        }

        protected virtual void InitializeStrategies()
        {
            if (isLocalPlayer && playerInput != null)
            {
                // Provide strategies from outside - using factory pattern
                SetMovementStrategy(CreateLocalMovementStrategy());
                SetAttackStrategy(CreateLocalAttackStrategy());
            }
            else
            {
                // Non-local players use externally defined strategies
                SetMovementStrategy(CreateNonLocalMovementStrategy());
                SetAttackStrategy(CreateNonLocalAttackStrategy());
            }
        }

        protected virtual IMovementStrategy CreateLocalMovementStrategy()
        {
            return new InputSystemMovementStrategy(playerInput);
        }


        protected virtual IAttackStrategy CreateLocalAttackStrategy()
        {
            return new InputSystemAttackStrategy(playerInput);
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
            _attackStrategy = strategy;
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

            if (_attackStrategy is InputSystemAttackStrategy inputAttack)
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
            if (_attackStrategy != null && _attackStrategy.CanExecute())
            {
                Model?.PARequested();
                _attackStrategy.Execute(Time.deltaTime);

                if (_attackStrategy is AutomatedAttackStrategy automatedAttack)
                {
                    automatedAttack.ResetCooldown();
                }
            }
        }

        protected override void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }
    }
}