using Assets.Scripts.Models;
using System.ComponentModel;
using UnityEngine;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Common;

namespace Assets.Scripts.Presenters
{
    [RequireComponent(typeof(Collider))]
    public class ColtBulletPresenter : PresenterBaseClass<ColtBullet>
    {
        private Collider _collider;

        protected override void Awake()
        {
            base.Awake();

            _collider = GetComponent<Collider>();
            if (_collider != null)
            {
                _collider.isTrigger = true;
            }

            // Start inactive: presenter instances are pooled and will be activated when used.
            gameObject.SetActive(false);
            if (_collider != null) _collider.enabled = false;
        }

        protected override void ModelSetInitialization(ColtBullet previousModel)
        {
            base.ModelSetInitialization(previousModel);

            if (previousModel != null)
            {
                previousModel.Expired -= OnBulletExpired;
                previousModel.PositionChanged -= OnPositionChanged;
            }

            if (Model != null)
            {
                Model.Expired += OnBulletExpired;
                Model.PositionChanged += OnPositionChanged;

                // Just ensure we start inactive when pool is created
                if (!Model.IsActive)
                {
                    gameObject.SetActive(false);
                    if (_collider != null) _collider.enabled = false;
                }
            }
            else
            {
                gameObject.SetActive(false);
                if (_collider != null) _collider.enabled = false;
            }
        }

        // Public method for ColtPresenter to activate this bullet
        public void ActivateBullet(Vector3 position, Quaternion rotation)
        {
            if (Model == null)
            {
                return;
            }

            // used for re-activation from pool because it may already be active
            Model.Expired -= OnBulletExpired;
            Model.PositionChanged -= OnPositionChanged;
            Model.Expired += OnBulletExpired;
            Model.PositionChanged += OnPositionChanged;

            transform.position = position;
            transform.rotation = rotation;

            // Activate GameObject and collider
            gameObject.SetActive(true);
            if (_collider != null) _collider.enabled = true;
        }

        private void OnBulletExpired(object sender, System.EventArgs e)
        {
            gameObject.SetActive(false);
        }

        private void OnPositionChanged(object sender, System.EventArgs e)
        {
            if (Model != null && gameObject.activeInHierarchy)
            {
                // Convert SerializableVector3 to Unity Vector3
                transform.position = ToUnityVector3(Model.Position);
            }
        }

        protected override void Update()
        {
            base.Update(); 
        }

        protected override void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!gameObject.activeInHierarchy || Model == null || !Model.IsActive)
            {
                return;
            }

            BrawlerPresenter targetPresenter = other.GetComponent<BrawlerPresenter>();
            if (targetPresenter != null && targetPresenter.Model != null)
            {
                // Convert Unity Vector3 to SerializableVector3 for the Model layer
                SerializableVector3 targetPosition = ToSerializableVector3(targetPresenter.transform.position);
                
                Model.DamageStrategy?.ApplyDamage(targetPresenter.Model, targetPosition, Model);

                // notify model it was hit; model raises Expired which the Colt model will handle to release.
                Model.MarkExpiredByHit();
            }
        }

        // Helper methods for conversion between Unity and Model types
        private Vector3 ToUnityVector3(SerializableVector3 sv3)
        {
            return new Vector3(sv3.X, sv3.Y, sv3.Z);
        }

        private SerializableVector3 ToSerializableVector3(Vector3 v3)
        {
            return new SerializableVector3(v3.x, v3.y, v3.z);
        }
    }
}