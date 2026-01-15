using Assets.Scripts.Models;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Assets.Scripts.Interfaces;


namespace Assets.Scripts.Presenters
{
    public class PD3StartGamePresenter : Singleton<PD3StartGamePresenter>
    {
        [Header("Brawler Prefabs")]
        [SerializeField] private GameObject coltPrefab;
        [SerializeField] private GameObject elPrimoPrefab;

        [Header("Spawn Points")]
        [SerializeField] private Transform[] spawnPoints;

        private PD3StarsGame _model;
        private readonly List<GameObject> _brawlerInstances = new List<GameObject>();
        private int _nextSpawnIndex = 0;

        public PD3StarsGame Model => _model;

        public enum BrawlerType
        {
            Colt,
            ElPrimo
        }

        protected override void Awake()
        {
            base.Awake(); 

            // Create game model
            _model = new PD3StarsGame();

            // Subscribe to model events
            _model.BrawlerAdded += OnBrawlerAdded;
            _model.BrawlerRemoved += OnBrawlerRemoved;

            // Spawn initial brawlers
            SpawnInitialBrawlers();
        }

        private void SpawnInitialBrawlers()
        {
            // Spawn local player (Colt)
            AddBrawler(BrawlerType.Colt, isLocalPlayer: true);

            // Spawn NPC brawlers
            AddBrawler(BrawlerType.ElPrimo, isLocalPlayer: false);
            AddBrawler(BrawlerType.Colt, isLocalPlayer: false);
        }

        private Transform GetNextSpawnPoint()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogWarning("No spawn points assigned! Using default position.");
                return null;
            }

            Transform spawnPoint = spawnPoints[_nextSpawnIndex % spawnPoints.Length];
            _nextSpawnIndex++;
            return spawnPoint;
        }

        public void AddBrawler(BrawlerType brawlerType, bool isLocalPlayer = false)
        {
            GameObject prefab = brawlerType switch
            {
                BrawlerType.Colt => coltPrefab,
                BrawlerType.ElPrimo => elPrimoPrefab,
                _ => null
            };

            if (prefab == null)
            {
                Debug.LogError($"Prefab for {brawlerType} not assigned!");
                return;
            }

            // Get spawn position
            Transform spawnPoint = GetNextSpawnPoint();
            Vector3 position = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            Quaternion rotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

            // Instantiate
            GameObject instance = Instantiate(prefab, position, rotation);
            
            // Configure based on local/NPC
            ConfigureBrawlerInstance(instance, isLocalPlayer);

            // Get presenter and add model to game
            var presenter = instance.GetComponent<BrawlerPresenter>();
            if (presenter != null && presenter.Model != null)
            {
                _model.AddBrawler(presenter.Model, isLocalPlayer);

                // Ensure HUDModel receives the player model immediately (handles cases where HUD expects IHUD)
                var hud = HUDModel.Instance;
                if (hud != null && presenter.Model is IHUD hudThing)
                {
                    if (hud.Slot1 != hudThing && hud.Slot2 != hudThing && hud.Slot3 != hudThing)
                    {
                        hud.AssignNext(hudThing);
                    }
                }
            }

            _brawlerInstances.Add(instance);
            Debug.Log($"Spawned {brawlerType} as {(isLocalPlayer ? "Local Player" : "NPC")}");
        }

        private void ConfigureBrawlerInstance(GameObject instance, bool isLocalPlayer)
        {
            if (isLocalPlayer)
            {
                instance.tag = "Player";
                var playerInput = instance.GetComponent<PlayerInput>();
                if (playerInput != null)
                {
                    playerInput.enabled = true;
                    playerInput.ActivateInput();
                }
            }
            else
            {
                instance.tag = "Untagged";
                var playerInput = instance.GetComponent<PlayerInput>();
                if (playerInput != null)
                {
                    playerInput.enabled = false;
                }
            }

            var presenter = instance.GetComponent<BrawlerPresenter>();
            presenter?.ForceInitializeStrategies();
        }

        private void OnBrawlerAdded(object sender, BrawlerAddedEventArgs e)
        {
            Debug.Log($"Brawler added to game: {e.Brawler.GetType().Name}, IsLocal: {e.IsLocalPlayer}");

            // Assign the first three brawlers to HUDModel slots (the model will raise slot changed events)
            var hudModel = HUDModel.Instance;
            if (hudModel != null)
            {
                if (e.Brawler is IHUD hudThing)
                {
                    int assigned = hudModel.AssignNext(hudThing);
                }
            }
        }

        private void OnBrawlerRemoved(object sender, BrawlerRemovedEventArgs e)
        {
            Debug.Log($"Brawler removed from game: {e.Brawler.GetType().Name}");
            // Optionally clear HUD slots when brawler removed
            var hudModel = HUDModel.Instance;
            if (hudModel != null)
            {
                if (e.Brawler is IHUD hudThing)
                {
                    // remove from matching slot
                    if (hudModel.Slot1 == hudThing) hudModel.ClearSlot(1);
                    else if (hudModel.Slot2 == hudThing) hudModel.ClearSlot(2);
                    else if (hudModel.Slot3 == hudThing) hudModel.ClearSlot(3);
                }
            }
        }

        protected override void OnDestroy()
        {
            if (_model != null)
            {
                _model.BrawlerAdded -= OnBrawlerAdded;
                _model.BrawlerRemoved -= OnBrawlerRemoved;
            }
            base.OnDestroy();
        }

        private void Update()
        {
            _model?.Update();
        }
    }
}