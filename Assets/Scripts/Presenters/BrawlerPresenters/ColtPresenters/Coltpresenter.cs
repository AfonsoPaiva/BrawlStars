using Assets.Scripts.Models;
using Assets.Scripts.Models.ColtModels;
using Assets.Scripts.Presenters.ColtPresenters;
using Assets.Scripts.Strategies.Attack;
using Assets.Scripts.Strategies.Movement;
using System.ComponentModel;
using UnityEngine;

namespace Assets.Scripts.Presenters
{
    public class ColtPresenter : BrawlerPresenter
    {
        [Header("Colt Settings")]
        [SerializeField] private GameObject coltBulletPrefab;
        [SerializeField] private float coltRotationSpeed = 180f;
        [SerializeField] private float automatedAttackInterval = 1.0f;

        public float ColtRotationSpeed => coltRotationSpeed;
        public float AutomatedAttackInterval => automatedAttackInterval;

        protected override void Awake()
        {
            base.Awake();
            Colt coltModel = new Colt();
            coltModel.ColtFired += OnColtFired;
            
            // Now set the model
            Model = coltModel;
        }

        protected override IMovementStrategy CreateNonLocalMovementStrategy()
        {
            return new RotationalMovementStrategy(coltRotationSpeed);
        }

        protected override IAttackStrategy CreateNonLocalAttackStrategy()
        {
            return new AutomatedAttackStrategy(automatedAttackInterval);
        }

        protected override void ModelSetInitialization(Brawler previousModel)
        {
            base.ModelSetInitialization(previousModel);

            // Unsubscribe from previous Colt
            if (previousModel is Colt previousColt)
            {
                previousColt.ColtFired -= OnColtFired;
            }

            // Subscribe to new Colt (if model was changed after initialization)
            if (Model is Colt colt && previousModel != null)
            {
                colt.ColtFired += OnColtFired;
            }
        }

        protected override void OnDestroy()
        {
            // Unsubscribe when destroyed
            if (Model is Colt colt)
            {
                colt.ColtFired -= OnColtFired;
            }
            
            base.OnDestroy();
        }

        private void OnColtFired(object sender, ColtBulletEventArgs e)
        {
            if (coltBulletPrefab != null)
            {
                Vector3 spawnPosition = transform.position + transform.forward * 1.5f;
                GameObject bulletGO = Instantiate(coltBulletPrefab, spawnPosition, transform.rotation);

                ColtBulletPresenter bulletPresenter = bulletGO.GetComponent<ColtBulletPresenter>();
                if (bulletPresenter != null)
                {
                    bulletPresenter.Model = e.Bullet;
                    // Pass the Colt model as the owner
                    bulletPresenter.Model.Initialize(spawnPosition, transform.forward, Model);
                }
            }
        }
    }
}