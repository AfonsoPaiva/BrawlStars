using Assets.Scripts.Models;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Strategies;

namespace Assets.Scripts.Presenters
{
    public class ColtPresenter : BrawlerPresenter
    {
        [Header("Colt Settings")]
        [SerializeField] private GameObject coltBulletPrefab;
        [SerializeField] private float coltRotationSpeed = 180f;
        [SerializeField] private float automatedAttackInterval = 0.1f;

        private readonly Dictionary<ColtBullet, ColtBulletPresenter> _bulletPresenterMap = new Dictionary<ColtBullet, ColtBulletPresenter>(Colt.TOTAL_BULLETS_SIZE);

        public float ColtRotationSpeed => coltRotationSpeed;
        public float AutomatedAttackInterval => automatedAttackInterval;

        protected override void Awake()
        {
            base.Awake();
            
            // Only create a new model if one doesn't exist (for backwards compatibility with non-command spawning)
            if (Model == null)
            {
                Model = new Colt();
            }
            
            // Override the base class attackInterval with Colt's specific interval
            attackInterval = automatedAttackInterval;
            
            Debug.Log($"ColtPresenter Awake: AutomatedAttackInterval: {automatedAttackInterval}, IsLocalPlayer: {IsLocalPlayer}");
        }

        protected override IMovementStrategy CreateNonLocalMovementStrategy()
        {
            return new RotationalMovementStrategy(coltRotationSpeed);
        }

        protected override IAttackStrategy CreateNonLocalAttackStrategy()
        {
            Debug.Log($"ColtPresenter: Creating AutomatedAttackStrategy with interval: {automatedAttackInterval}");
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

                // Initialize bullet pool presenters
                InitializeBulletPool(colt);
            }
        }

        private void InitializeBulletPool(Colt colt)
        {
            if (coltBulletPrefab == null)
            {
                Debug.LogError("ColtPresenter: coltBulletPrefab is not assigned!");
                return;
            }

            var bullets = colt.BulletPool;
            if (bullets == null)
            {
                Debug.LogError("ColtPresenter: BulletPool is null!");
                return;
            }

            // Clear existing map in case we're reinitializing
            _bulletPresenterMap.Clear();

            // Destroy any existing bullet GameObjects (from previous model)
            foreach (Transform child in transform)
            {
                if (child.GetComponent<ColtBulletPresenter>() != null)
                {
                    Destroy(child.gameObject);
                }
            }

            // Create presenters for all bullets in the pool
            for (int i = 0; i < bullets.Count; i++)
            {
                var modelBullet = bullets[i];

                // Instantiate as child of this ColtPresenter transform
                GameObject bulletGO = Instantiate(coltBulletPrefab, transform);
                bulletGO.transform.localPosition = Vector3.zero;
                bulletGO.transform.localRotation = Quaternion.identity;
                bulletGO.SetActive(false);

                ColtBulletPresenter bulletPresenter = bulletGO.GetComponent<ColtBulletPresenter>();

                if (bulletPresenter == null)
                {
                    Debug.LogError($"ColtPresenter: Bullet prefab is missing ColtBulletPresenter component!");
                    Destroy(bulletGO);
                    continue;
                }

                // Assign model instance (presenter will subscribe but remain inactive until fired)
                bulletPresenter.Model = modelBullet;
                _bulletPresenterMap[modelBullet] = bulletPresenter;
            }

            Debug.Log($"ColtPresenter: Initialized bullet pool with {_bulletPresenterMap.Count} bullet presenters for ModelID={colt.ModelID}");
        }

        protected override void OnDestroy()
        {
            if (Model is Colt colt)
            {
                colt.ColtFired -= OnColtFired;
            }

            // Cleanup bullet presenters
            foreach (var kvp in _bulletPresenterMap)
            {
                if (kvp.Value != null && kvp.Value.gameObject != null)
                {
                    Destroy(kvp.Value.gameObject);
                }
            }

            _bulletPresenterMap.Clear();

            base.OnDestroy();
        }

        private void OnColtFired(object sender, ColtBulletEventArgs e)
        {
            if (e?.Bullet == null)
            {
                Debug.LogWarning("ColtPresenter: ColtBulletEventArgs or Bullet is null!");
                return;
            }

            if (!_bulletPresenterMap.TryGetValue(e.Bullet, out ColtBulletPresenter bulletPresenter))
            {
                Debug.LogError($"ColtPresenter: Bullet not found in presenter map! Map has {_bulletPresenterMap.Count} entries, Model has {((Colt)Model).BulletPool.Count} bullets");
                return;
            }

            if (bulletPresenter == null)
            {
                Debug.LogError("ColtPresenter: BulletPresenter in map is null!");
                return;
            }

            // Calculate spawn position & orientation in world space
            Vector3 spawnPosition = transform.position + transform.forward * 1.5f;
            Quaternion spawnRotation = transform.rotation;

            // Initialize model FIRST because the presenter uses the model's data in its ActivateBullet
            bulletPresenter.Model.Initialize(spawnPosition, transform.forward, Model);

            // THEN activate the presenter GameObject and position it
            bulletPresenter.ActivateBullet(spawnPosition, spawnRotation);

            Debug.Log($"ColtPresenter: Fired bullet from {transform.position}");
        }
    }
}