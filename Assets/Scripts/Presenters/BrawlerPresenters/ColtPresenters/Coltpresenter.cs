using Assets.Scripts.Models;
using Assets.Scripts.Models.ColtModels;
using Assets.Scripts.Presenters.ColtPresenters;
using Assets.Scripts.Strategies.Attack;
using Assets.Scripts.Strategies.Movement;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Presenters
{
    public class ColtPresenter : BrawlerPresenter
    {
        [Header("Colt Settings")]
        [SerializeField] private GameObject coltBulletPrefab;
        [SerializeField] private float coltRotationSpeed = 180f;
        [SerializeField] private float automatedAttackInterval = 0.1f;

        private readonly Dictionary<ColtBullet, ColtBulletPresenter> _bulletPresenterMap = new Dictionary<ColtBullet, ColtBulletPresenter>(Colt.TOTAL_BULLETS_SIZE);
        private bool _poolInitialized = false;

        public float ColtRotationSpeed => coltRotationSpeed;
        public float AutomatedAttackInterval => automatedAttackInterval;

        protected override void Awake()
        {
            base.Awake();
            Model = new Colt();
            
            // Override the base class attackInterval with Colt's specific interval
            attackInterval = automatedAttackInterval;
            
            Debug.Log($"Colt AutomatedAttackInterval: {automatedAttackInterval}");
        }

        protected override IMovementStrategy CreateNonLocalMovementStrategy()
        {
            return new RotationalMovementStrategy(coltRotationSpeed);
        }

        protected override IAttackStrategy CreateNonLocalAttackStrategy()
        {
            Debug.Log($"Creating AutomatedAttackStrategy with interval: {automatedAttackInterval}");
            return new AutomatedAttackStrategy(automatedAttackInterval);
        }

        protected override void ModelSetInitialization(Brawler previousModel)
        {
            base.ModelSetInitialization(previousModel);

            // Unsubscribe from previous model
            if (previousModel is Colt previousColt)
            {
                previousColt.ColtFired -= OnColtFired;
            }

            if (Model is Colt colt)
            {
                colt.ColtFired -= OnColtFired; // defensive
                colt.ColtFired += OnColtFired;

                // Only create the pool once at the beginning
                if (!_poolInitialized)
                {
                    if (coltBulletPrefab == null)
                    {
                        return;
                    }

                    var bullets = colt.BulletPool;
                    if (bullets == null)
                    {
                        return;
                    }

                    // instantiate exactly the number of presenters required as children of this ColtPresenter
                    for (int i = 0; i < bullets.Count; i++)
                    {
                        var modelBullet = bullets[i];

                        // Instantiate as child of this ColtPresenter transform
                        GameObject bulletGO = Instantiate(coltBulletPrefab, transform);
                        bulletGO.transform.localPosition = Vector3.zero;
                        bulletGO.transform.localRotation = Quaternion.identity;
                        bulletGO.SetActive(false);

                        ColtBulletPresenter bulletPresenter = bulletGO.GetComponent<ColtBulletPresenter>();

                    

                        // assign model instance (presenter will subscribe but remain inactive until fired)
                        bulletPresenter.Model = modelBullet;
                        _bulletPresenterMap[modelBullet] = bulletPresenter;
                    }

                    _poolInitialized = true;
                }
            }
        }


        // updated this fot the pooling system
        protected override void OnDestroy()
        {
            if (Model is Colt colt)
            {
                colt.ColtFired -= OnColtFired;
            }

            _bulletPresenterMap.Clear();

            base.OnDestroy();
        }

        private void OnColtFired(object sender, ColtBulletEventArgs e)
        {
            if (e?.Bullet == null)
            {
                return;
            }

            if (!_bulletPresenterMap.TryGetValue(e.Bullet, out ColtBulletPresenter bulletPresenter))
            {
                return;
            }

            if (bulletPresenter == null)
            {
                return;
            }

            // Calculate spawn position & orientation in world space
            Vector3 spawnPosition = transform.position + transform.forward * 1.5f;
            Quaternion spawnRotation = transform.rotation;


            // Initialize model FIRST because the presenter uses the model's data in its ActivateBullet
            bulletPresenter.Model.Initialize(spawnPosition, transform.forward, Model);

            // THEN activate the presenter GameObject and position it becaues the presenter uses the model's position in its Update
            bulletPresenter.ActivateBullet(spawnPosition, spawnRotation);

        }
    }
}