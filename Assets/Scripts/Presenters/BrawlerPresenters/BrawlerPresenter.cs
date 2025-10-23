using Assets.Scripts.Models;
using Assets.Scripts.Models.ColtModels;
using JetBrains.Annotations;
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

        [Header("Health Bar Settings")]
        [SerializeField] private VisualTreeAsset healthBarTemplate;
        [SerializeField] private UIDocument _hudDOcument;
        [SerializeField] private Transform healthBarAnchor;

        private PD3HealthBars.HealthBarPresenter _healthBarPresenter;

        protected Vector2 _moveDirection;


        protected override void Awake() {   }
        protected void Start()
        {
            ADDHB(_hudDOcument, healthBarTemplate);
            if (playerInput != null)
            {
                playerInput.actions["Move"].performed += Move_performed;
                playerInput.actions["Move"].canceled += Move_canceled;
                playerInput.actions["Attack"].performed += PrimaryAttack_performed;
            }
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

        protected override void OnDestroy()
        {
            if (playerInput != null)
            {
                playerInput.actions["Move"].performed -= Move_performed;
                playerInput.actions["Move"].canceled -= Move_canceled;
                playerInput.actions["Attack"].performed -= PrimaryAttack_performed;
            }

            base.OnDestroy();
        }

        protected virtual void Move_performed(InputAction.CallbackContext context)
        {
            _moveDirection = context.ReadValue<Vector2>();
        }

        protected virtual void Move_canceled(InputAction.CallbackContext context)
        {
            _moveDirection = Vector2.zero;
        }

        protected virtual void PrimaryAttack_performed(InputAction.CallbackContext context)
        {
            Model?.PARequested();
        }

        protected override void Update()
        {
            base.Update();
            HandleMovement();
            Debug.Log($"{Model.GetType().Name} Position: {transform.position}");
            Debug.Log($"{Model.GetType().Name} MoveDirection: {_moveDirection}");

        }

        private void LateUpdate()
        {
            if (playerInput != null) 
                _healthBarPresenter.UpdatePosition();
        
        }

        protected virtual void HandleMovement()
        {
            if (_moveDirection.sqrMagnitude > 0.01f)
            {
                // Move
                Vector3 movement = new Vector3(_moveDirection.x, 0, _moveDirection.y);
                transform.position += movement * moveSpeed * Time.deltaTime;

                // Rotate
                Quaternion targetRotation = Quaternion.LookRotation(movement);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        protected override void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }
    }
}