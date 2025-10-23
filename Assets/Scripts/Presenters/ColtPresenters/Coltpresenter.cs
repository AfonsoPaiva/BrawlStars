using Assets.Scripts.Models;
using Assets.Scripts.Models.ColtModels;
using Assets.Scripts.Presenters.ColtPresenters;
using System.ComponentModel;
using UnityEngine;

namespace Assets.Scripts.Presenters
{
    public class ColtPresenter : BrawlerPresenter
    {
        [Header("Colt Settings")]
        [SerializeField] private GameObject coltBulletPrefab;

        protected override void Awake()
        {
            base.Awake();
            Model = new Colt();
        }

        protected override void ModelSetInitialization(Brawler previousModel)
        {
            base.ModelSetInitialization(previousModel);

            // Unsubscribe from previous Colt
            if (previousModel is Colt previousColt)
            {
                previousColt.ColtFired -= OnColtFired;
            }

            // Subscribe to new Colt
            if (Model is Colt colt)
            {
                colt.ColtFired += OnColtFired;
            }
        }

        private void OnColtFired(object sender, ColtBulletEventArgs e)
        {
            if (coltBulletPrefab != null)
            {
                // SPAWN BULLET IN FRONT OF COLT
                Vector3 spawnPosition = transform.position + transform.forward * 1.5f;
                GameObject bulletGO = Instantiate(coltBulletPrefab, spawnPosition, transform.rotation);

                // SET BULLET MODEL AND INITIALIZE
                ColtBulletPresenter bulletPresenter = bulletGO.GetComponent<ColtBulletPresenter>();
                if (bulletPresenter != null)
                {
                    bulletPresenter.Model = e.Bullet;

                    // Initialize the bullet model with position and direction
                    bulletPresenter.Model.Initialize(spawnPosition, transform.forward);
                }
            }
        }
    }
}