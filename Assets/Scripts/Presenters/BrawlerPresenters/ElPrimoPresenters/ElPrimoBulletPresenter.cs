using Assets.Scripts.Models.ColtModels;
using Assets.Scripts.Models.ElPrimoModels;
using Assets.Scripts.Strategies.Damage;
using System.ComponentModel;
using UnityEngine;

namespace Assets.Scripts.Presenters.ElPrimoPresenters
{
    [RequireComponent(typeof(Collider))]
    public class ElPrimoBulletPresenter : PresenterBaseClass<ElPrimoBullet>
    {
        private IDamageStrategy _damageStrategy;

        protected override void Awake()
        {
            base.Awake();

            // Initialize with standard damage strategy
            _damageStrategy = new StandardDamageStrategy();

            // Ensure collider is set to trigger
            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }

            if (Model != null)
            {
                Model.Expired += OnBulletExpired;
                Model.PositionChanged += OnPositionChanged;
            }
        }

        protected override void ModelSetInitialization(ElPrimoBullet previousModel)
        {
            base.ModelSetInitialization(previousModel);

            // Unsubscribe from previous model
            if (previousModel != null)
            {
                previousModel.Expired -= OnBulletExpired;
                previousModel.PositionChanged -= OnPositionChanged;
            }

            // Subscribe to new model
            if (Model != null)
            {
                Model.Expired += OnBulletExpired;
                Model.PositionChanged += OnPositionChanged;

                // Set initial position from model
                transform.position = Model.Position;
            }
        }

        public void SetDamageStrategy(IDamageStrategy strategy)
        {
            _damageStrategy = strategy ?? new StandardDamageStrategy();
        }

        private void OnTriggerEnter(Collider other)
        {
            // Check if the collided object is a brawler
            BrawlerPresenter targetPresenter = other.GetComponent<BrawlerPresenter>();
            if (targetPresenter != null && targetPresenter.Model != null)
            {
                // Don't damage the owner
                if (Model.Owner != null && targetPresenter.Model == Model.Owner)
                {
                    return;
                }

                // Apply damage using the strategy, passing both source and target GameObjects
                _damageStrategy?.ApplyDamage(targetPresenter.Model, Model.Damage, gameObject, targetPresenter.gameObject);

                // Destroy the bullet after hitting a target
                Destroy(gameObject);
            }
        }

        private void OnBulletExpired(object sender, System.EventArgs e)
        {
            Destroy(gameObject);
        }

        private void OnPositionChanged(object sender, System.EventArgs e)
        {
            // PRESENTER updates the view based on model changes
            transform.position = Model.Position;
        }

        protected override void Update()
        {
            base.Update();
        }

        protected override void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }
    }
}