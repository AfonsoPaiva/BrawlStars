using Assets.Scripts.Models;
using System;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Healthbars
{
    public class LocalPlayerHUDPresenter : MonoBehaviour
    {
        [Header("Canvas / TextMeshPro ")]
        [SerializeField] private Canvas hudCanvas;
        [SerializeField] private TextMeshProUGUI lifeText;

        // Model reference (will be bound by game presenter)
        private Brawler _localBrawler;
        private PD3StarsGame _gameModel;

        private void Start()
        {
            // Start with canvas hidden until a local brawler exists
            if (hudCanvas != null)
            {
                hudCanvas.gameObject.SetActive(false);
            }

            // No singleton lookup here — model must be injected via BindModel
        }

        // Called by the game presenter to inject the model (decouples HUD from presenter singleton)
        public void BindModel(PD3StarsGame model)
        {
            if (_gameModel != null)
            {
                _gameModel.LocalBrawlerChanged -= OnLocalBrawlerChanged;
            }

            _gameModel = model;

            if (_gameModel != null)
            {
                _gameModel.LocalBrawlerChanged += OnLocalBrawlerChanged;

                if (_gameModel.LocalBrawler != null)
                {
                    SetLocalBrawler(_gameModel.LocalBrawler);
                }
            }
        }

        private void OnLocalBrawlerChanged(object sender, EventArgs e)
        {
            var localBrawler = _gameModel?.LocalBrawler;
            SetLocalBrawler(localBrawler);
        }

        private void SetLocalBrawler(Brawler brawler)
        {
            if (_localBrawler != null)
            {
                _localBrawler.HealthChanged -= OnHealthChanged;
                _localBrawler.Died -= OnLocalPlayerDied;
            }

            _localBrawler = brawler;

            if (_localBrawler != null)
            {
                _localBrawler.HealthChanged += OnHealthChanged;
                _localBrawler.Died += OnLocalPlayerDied;

                if (hudCanvas != null)
                {
                    hudCanvas.gameObject.SetActive(true);
                }

                UpdateHealthDisplay();
                Debug.Log($"LocalPlayerHUDPresenter: Now tracking {_localBrawler.GetType().Name}");
            }
            else
            {
                if (hudCanvas != null)
                {
                    hudCanvas.gameObject.SetActive(false);
                }
            }
        }

        private void OnHealthChanged(object sender, EventArgs e)
        {
            UpdateHealthDisplay();
        }

        private void OnLocalPlayerDied(object sender, EventArgs e)
        {
            UpdateHealthDisplay();
            if (hudCanvas != null)
            {
                hudCanvas.gameObject.SetActive(false);
            }
        }

        private void UpdateHealthDisplay()
        {
            if (_localBrawler == null)
            {
                if (lifeText != null)
                {
                    lifeText.text = string.Empty;
                }

                return;
            }

            if (lifeText != null)
            {
                lifeText.text = $"life : {_localBrawler.Health:F0}";
            }
            else
            {
                Debug.LogWarning("LocalPlayerHUDPresenter: TextMeshProUGUI (lifeText) not assigned in Inspector.");
            }
        }

        private void OnDestroy()
        {
            if (_localBrawler != null)
            {
                _localBrawler.HealthChanged -= OnHealthChanged;
                _localBrawler.Died -= OnLocalPlayerDied;
            }

            if (_gameModel != null)
            {
                _gameModel.LocalBrawlerChanged -= OnLocalBrawlerChanged;
            }
        }
    }
}
