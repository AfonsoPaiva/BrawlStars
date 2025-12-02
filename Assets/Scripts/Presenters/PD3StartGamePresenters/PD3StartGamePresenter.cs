using Assets.Scripts.Models;
using Assets.Scripts.Models.ColtModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using System.Collections;
using Assets.Scripts.Models.ElPrimoModels;
using UnityEngine.InputSystem;
using Assets.Scripts.Strategies.Attack;
using Assets.Scripts.Strategies.Movement;

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

        [Header("Existing Scene Player")]
        [SerializeField] private GameObject existingColtPlayer; 

        private float _currentSpawnDelay;
        private float _maxSpawnDelay = 3f;
        private int _maxSpawnCounter = 2;
        private int _currentSpawnCounter = 0;

        private List<GameObject> brawlerInstances = new List<GameObject>();
        private bool _hasConfiguredLocalPlayer = false;

        // Helper enum to specify brawler type without creating model instance
        public enum BrawlerType
        {
            Colt,
            ElPrimo
        }

        protected override void Awake()
        {
            base.Awake();
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            ConfigureExistingScenePlayer();
        }

        private void ConfigureExistingScenePlayer()
        {
            if (existingColtPlayer != null && !_hasConfiguredLocalPlayer)
            {
                var presenter = existingColtPlayer.GetComponent<BrawlerPresenter>();
                if (presenter != null)
                {
                    existingColtPlayer.tag = "Player";
                    var playerInput = existingColtPlayer.GetComponent<PlayerInput>();
                    if (playerInput != null)
                    {
                        playerInput.enabled = true;
                        playerInput.ActivateInput();
                    }

                    // Initialize strategies
                    presenter.ForceInitializeStrategies();
                    _hasConfiguredLocalPlayer = true;

                    Debug.Log("Configured existing scene Colt as local player");
                }
            }
        }

        protected override void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            base.OnDestroy();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (Instance != null)
            {
                SpawnBrawlerNetworkSim();
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
                    AddBrawler(BrawlerType.Colt, false); 
                }
                else
                {
                    AddBrawler(BrawlerType.ElPrimo, false); 
                }
                _currentSpawnDelay -= _maxSpawnDelay;
                _currentSpawnCounter++;
            }
        }

        public void AddBrawler(BrawlerType brawlerType, bool isLocalPlayer = false)
        {
            if (isLocalPlayer && _hasConfiguredLocalPlayer)
            {
                Debug.LogWarning("Local player already exists in scene. Not spawning another one.");
                return;
            }

            GameObject prefab = null;
            Transform spawnPoint = null;

            switch (brawlerType)
            {
                case BrawlerType.Colt:
                    prefab = coltPrefab;
                    spawnPoint = coltSpawnPoint;
                    break;
                case BrawlerType.ElPrimo:
                    prefab = elPrimoPrefab;
                    spawnPoint = elPrimoSpawnPoint;
                    break;
            }

            if (prefab != null && spawnPoint != null)
            {
                var instance = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

                // Configure as NPC (local player already exists in scene)
                instance.tag = "Untagged";

                // Disable PlayerInput for NPCs
                var instancePlayerInput = instance.GetComponent<PlayerInput>();
                if (instancePlayerInput != null)
                {
                    instancePlayerInput.enabled = false;
                }

                var presenter = instance.GetComponent<BrawlerPresenter>();
                if (presenter != null)
                {
                    // DON'T set Model - presenter already creates it in Awake()!
                    // presenter.Model = brawlerModel; // ❌ REMOVED - This was creating duplicate!

                    presenter.ForceInitializeStrategies();
                }

                brawlerInstances.Add(instance);

                Debug.Log($"Spawned {brawlerType} as NPC");
            }
        }

        protected override void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }
    }
}