using Assets.Scripts.Models.ColtModels;
using Assets.Scripts.Models;
using System.ComponentModel;
using UnityEngine;

namespace Assets.Scripts.Presenters.ColtPresenters
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
                transform.position = Model.Position;
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
                // Pass target's transform position from presenter to strategy
                Model.DamageStrategy?.ApplyDamage(targetPresenter.Model, targetPresenter.transform.position, Model);

                // notify model it was hit; model raises Expired which the Colt model will handle to release.
                Model.MarkExpiredByHit();
            }
        }
    }
}