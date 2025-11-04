using Assets.Scripts.Models;
using Assets.Scripts.Models.ColtModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using System.Collections;
using Assets.Scripts.Models.ElPrimoModels;

namespace Assets.Scripts.Presenters
{
    internal class PD3StartGamePresenter : PresenterBaseClass<PD3StarsGame>
    {
        public static PD3StartGamePresenter Instance { get; private set; }

        [Header("Brawler Prefabs")]
        [SerializeField] private GameObject coltPrefab;
        [SerializeField] private GameObject elPrimoPrefab;

        [Header("Spawn Points")]
        [SerializeField] private Transform coltSpawnPoint;
        [SerializeField] private Transform elPrimoSpawnPoint;

        private float _currentSpawnDelay;
        private float _maxSpawnDelay = 3f;
        private int _maxSpawnCounter = 2; 
        private int _currentSpawnCounter = 0;

        private List<GameObject> brawlerInstances = new List<GameObject>();

        protected override void Awake()
        {
            base.Awake();
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void FixedUpdate()
        {
            if (Instance != null)
            {
                {
                    SpawnBrawlerNetworkSim();
                }
            }
        }

        private void SpawnBrawlerNetworkSim()
        {
            if (_currentSpawnCounter >= _maxSpawnCounter) return;
            _currentSpawnDelay += Time.fixedDeltaTime;
            if (_currentSpawnDelay > _maxSpawnDelay)
            {
                if (_currentSpawnCounter % 2 == 0)
                {
                    AddBrawler(new Colt());
                }
                else
                {
                    AddBrawler(new ElPrimo());
                }
                _currentSpawnDelay -= _maxSpawnDelay;
                _currentSpawnCounter++;
            }
        }

        public void AddBrawler(Brawler brawlerModel)
        {
            GameObject prefab = null;
            Transform spawnPoint = null;

            if (brawlerModel is Colt)
            {
                prefab = coltPrefab;
                spawnPoint = coltSpawnPoint;
            }
            else if (brawlerModel is ElPrimo)
            {
                prefab = elPrimoPrefab;
                spawnPoint = elPrimoSpawnPoint;
            }

            if (prefab != null && spawnPoint != null)
            {
                var instance = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
                var presenter = instance.GetComponent<BrawlerPresenter>();
                if (presenter != null)
                {
                    presenter.Model = brawlerModel;
                    
                    // Only the first brawler is the local player
                    // Set isLocalPlayer in the Unity Inspector or programmatically here
                    // Note: You need to make isLocalPlayer publicly settable
                }
                brawlerInstances.Add(instance);
            }
        }

        protected override void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            
        }
    }
}