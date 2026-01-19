using Assets.Scripts.Common;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;
using Assets.Scripts.Strategies;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Presenters
{
    public class ElPrimopresenter : BrawlerPresenter
    {
        [Header("ElPrimo Settings")]
        [SerializeField] private float elPrimoRotationSpeed = 180f;
        [SerializeField] private float automatedAttackInterval = 1.0f;

        [Header("Dash Attack Settings")]
        [SerializeField] private float dashDistance = 5f;
        [SerializeField] private float dashDuration = 0.3f;
        [SerializeField] private AnimationCurve dashCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;

        private Vector3Animation _currentDashAnimation;
        private InputAction _dashAction;

        public float ElPrimoRotationSpeed => elPrimoRotationSpeed;
        public float AutomatedAttackInterval => automatedAttackInterval;

        protected override void Awake()
        {
            base.Awake();

            // Only create a new model if one doesn't exist
            if (Model == null)
            {
                Model = new ElPrimo();
                Debug.Log($"[ElPrimopresenter] Created new ElPrimo model in Awake");
            }
            else
            {
                Debug.Log($"[ElPrimopresenter] Using existing ElPrimo model with ModelID={Model.ModelID}");
            }
        }

        protected virtual IMovementStrategy CreateNonLocalMovementStrategy()
        {
            return new RotationalMovementStrategy(elPrimoRotationSpeed);
        }

        protected virtual IAttackStrategy CreateNonLocalAttackStrategy()
        {
            return new AutomatedAttackStrategy(automatedAttackInterval);
        }

        protected override void InitializeStrategies()
        {
            Debug.Log($"[ElPrimopresenter] InitializeStrategies called for {gameObject.name}");

            // Call base to set up movement and damage strategies
            base.InitializeStrategies();

            // Override attack strategy if Dash type is selected
            if (AttackType == AttackExecutionType.Dash)
            {
                Debug.Log($"[{name}] Initializing Dash attack for ElPrimo");
                // Dash is handled directly in this presenter through DashRequested event
                // No attack strategy needed
                SetAttackStrategy(null);
            }
        }

        protected override void ModelSetInitialization(Brawler previousModel)
        {
            base.ModelSetInitialization(previousModel);

            Debug.Log($"[ElPrimopresenter] ModelSetInitialization - Model: {Model?.GetType().Name}, ModelID: {Model?.ModelID}");

            // Subscribe to dash events if this is an ElPrimo
            if (Model is ElPrimo elPrimo)
            {
                elPrimo.DashRequested += OnDashRequested;
                Debug.Log($"[ElPrimopresenter] Subscribed to DashRequested for ModelID {elPrimo.ModelID}");
            }

            // Unsubscribe from previous model
            if (previousModel is ElPrimo prevPrimo)
            {
                prevPrimo.DashRequested -= OnDashRequested;
            }

            // Setup input for dash if local player
            var playerInput = GetComponent<PlayerInput>();
            if (playerInput != null && playerInput.enabled)
            {
                _dashAction = playerInput.actions.FindAction("Dash", false);
                if (_dashAction == null)
                {
                    Debug.LogWarning("No 'Dash' action found in Input Actions. Using keyboard fallback.");
                }
                else
                {
                    _dashAction.performed += OnDashInput;
                    Debug.Log("Dash input action configured");
                }
            }
        }

        private void OnDashInput(InputAction.CallbackContext context)
        {
            ExecuteDash();
        }

        protected override void Update()
        {
            base.Update();

            // Update dash animation if active
            if (_currentDashAnimation != null && _currentDashAnimation.IsPlaying)
            {
                _currentDashAnimation.Update(Time.deltaTime);

                // Apply animation value to transform
                transform.position = _currentDashAnimation.CurrentUnityValue;
            }

            // Handle dash input based on attack type
            if (AttackType == AttackExecutionType.Dash)
            {
                // Keyboard fallback for dash
                if (_dashAction == null && Input.GetKeyDown(dashKey) && IsLocalPlayer)
                {
                    ExecuteDash();
                }
                // Or if dash action exists and is triggered
                else if (_dashAction != null && _dashAction.triggered)
                {
                    ExecuteDash();
                }
            }
        }

        protected override void HandleAttack()
        {
            // If Dash attack type is selected, don't use base attack handling
            if (AttackType == AttackExecutionType.Dash)
            {
                // Dash progress is handled by animation
                if (Model != null && _currentDashAnimation != null)
                {
                    Model.PAProgress = _currentDashAnimation.Progress;
                }
                return;
            }

            // Otherwise use base attack handling
            base.HandleAttack();
        }

        private void ExecuteDash()
        {
            if (Model is ElPrimo elPrimo && !elPrimo.IsDashing)
            {
                // Calculate dash direction based on forward direction
                Vector3 dashDirection = transform.forward.normalized;

                // Convert to SerializableVector3 for Model layer
                var serializableDir = new SerializableVector3(
                    dashDirection.x,
                    dashDirection.y,
                    dashDirection.z
                );

                // Request dash from model
                elPrimo.RequestDash(serializableDir, dashDistance);

                Debug.Log($"[ElPrimopresenter] Dash requested in direction {dashDirection}");
            }
        }

        private void OnDashRequested(object sender, DashRequestedEventArgs e)
        {
            // Create dash animation in Presenter layer
            Vector3 startPos = transform.position;
            Vector3 direction = new Vector3(e.Direction.X, e.Direction.Y, e.Direction.Z);
            Vector3 endPos = startPos + direction * e.Distance;

            _currentDashAnimation = new Vector3Animation(startPos, endPos, dashDuration, dashCurve);

            // Set animation in model so it can observe events
            if (Model is ElPrimo elPrimo)
            {
                elPrimo.SetDashAnimation(_currentDashAnimation);
            }

            // Start the animation
            _currentDashAnimation.Play();

            Debug.Log($"ElPrimo dash animation started: {startPos} -> {endPos}");
        }

        protected override void OnDestroy()
        {
            if (Model is ElPrimo elPrimo)
            {
                elPrimo.DashRequested -= OnDashRequested;
            }

            if (_dashAction != null)
            {
                _dashAction.performed -= OnDashInput;
            }

            base.OnDestroy();
        }
    }
}