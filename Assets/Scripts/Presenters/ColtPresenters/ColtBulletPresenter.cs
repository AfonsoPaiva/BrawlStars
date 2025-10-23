using Assets.Scripts.Models.ColtModels;
using System.ComponentModel;
using UnityEngine;

namespace Assets.Scripts.Presenters.ColtPresenters
{
    public class ColtBulletPresenter : PresenterBaseClass<ColtBullet>
    {
        protected override void Awake()
        {
            base.Awake();

            if (Model != null)
            {
                Model.Expired += OnBulletExpired;
                Model.PositionChanged += OnPositionChanged;
            }
        }

        protected override void ModelSetInitialization(ColtBullet previousModel)
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