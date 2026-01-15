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
        [SerializeField] private TextMeshProUGUI paText;

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
            else
            {
                Debug.LogError("LocalPlayerHUDPresenter: hudCanvas is not assigned in Inspector!");
            }

            if (lifeText == null)
            {
                Debug.LogError("LocalPlayerHUDPresenter: lifeText is not assigned in Inspector!");
            }

            if (paText == null)
            {
                Debug.LogError("LocalPlayerHUDPresenter: paText is not assigned in Inspector!");
            }
        }

        // Called by the game presenter to inject the model (decouples HUD from presenter singleton)
        public void BindModel(PD3StarsGame model)
        {
            Debug.Log($"LocalPlayerHUDPresenter: BindModel called with model: {(model != null ? "valid" : "null")}");

            if (_gameModel != null)
            {
                _gameModel.LocalBrawlerChanged -= OnLocalBrawlerChanged;
            }

            _gameModel = model;

            if (_gameModel != null)
            {
                _gameModel.LocalBrawlerChanged += OnLocalBrawlerChanged;
                Debug.Log("LocalPlayerHUDPresenter: Subscribed to LocalBrawlerChanged event");

                if (_gameModel.LocalBrawler != null)
                {
                    Debug.Log($"LocalPlayerHUDPresenter: LocalBrawler already exists: {_gameModel.LocalBrawler.GetType().Name}");
                    SetLocalBrawler(_gameModel.LocalBrawler);
                }
                else
                {
                    Debug.Log("LocalPlayerHUDPresenter: No LocalBrawler yet, waiting for LocalBrawlerChanged event");
                }
            }
        }

        private void OnLocalBrawlerChanged(object sender, EventArgs e)
        {
            Debug.Log($"LocalPlayerHUDPresenter: OnLocalBrawlerChanged fired! LocalBrawler: {_gameModel?.LocalBrawler?.GetType().Name ?? "null"}");
            var localBrawler = _gameModel?.LocalBrawler;
            SetLocalBrawler(localBrawler);
        }

        private void SetLocalBrawler(Brawler brawler)
        {
            Debug.Log($"LocalPlayerHUDPresenter: SetLocalBrawler called with: {brawler?.GetType().Name ?? "null"}");

            if (_localBrawler != null)
            {
                _localBrawler.HealthChanged -= OnHealthChanged;
                _localBrawler.PAProgressChanged -= OnPAProgressChanged;
                _localBrawler.Died -= OnLocalPlayerDied;
            }

            _localBrawler = brawler;

            if (_localBrawler != null)
            {
                _localBrawler.HealthChanged += OnHealthChanged;
                _localBrawler.PAProgressChanged += OnPAProgressChanged;
                _localBrawler.Died += OnLocalPlayerDied;

                if (hudCanvas != null)
                {
                    hudCanvas.gameObject.SetActive(true);
                    Debug.Log("LocalPlayerHUDPresenter: Canvas activated");
                }
                else
                {
                    Debug.LogError("LocalPlayerHUDPresenter: Cannot activate canvas - hudCanvas is null!");
                }

                // Force initial update to display current values immediately
                UpdateHealthDisplay();
                UpdatePADisplay();
                Debug.Log($"LocalPlayerHUDPresenter: Now tracking {_localBrawler.GetType().Name} with Health: {_localBrawler.Health}, PA: {_localBrawler.PAProgress}");
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
            Debug.Log($"LocalPlayerHUDPresenter: OnHealthChanged - New Health: {_localBrawler?.Health}");
            UpdateHealthDisplay();
        }

        private void OnPAProgressChanged(object sender, EventArgs e)
        {
            Debug.Log($"LocalPlayerHUDPresenter: OnPAProgressChanged - New PA: {_localBrawler?.PAProgress}");
            UpdatePADisplay();
        }

        private void OnLocalPlayerDied(object sender, EventArgs e)
        {
            UpdateHealthDisplay();
            UpdatePADisplay();
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
                lifeText.text = $"Life: {_localBrawler.Health:F0}";
                Debug.Log($"LocalPlayerHUDPresenter: Updated lifeText to: {lifeText.text}");
            }
            else
            {
                Debug.LogError("LocalPlayerHUDPresenter: lifeText is null - cannot update!");
            }
        }

        private void UpdatePADisplay()
        {
            if (_localBrawler == null)
            {
                if (paText != null)
                {
                    paText.text = string.Empty;
                }
                return;
            }

            if (paText != null)
            {
                paText.text = $"PA: {_localBrawler.PAProgress * 100:F0}%";
                Debug.Log($"LocalPlayerHUDPresenter: Updated paText to: {paText.text}");
            }
            else
            {
                Debug.LogError("LocalPlayerHUDPresenter: paText is null - cannot update!");
            }
        }

        private void OnDestroy()
        {
            if (_localBrawler != null)
            {
                _localBrawler.HealthChanged -= OnHealthChanged;
                _localBrawler.PAProgressChanged -= OnPAProgressChanged;
                _localBrawler.Died -= OnLocalPlayerDied;
            }

            if (_gameModel != null)
            {
                _gameModel.LocalBrawlerChanged -= OnLocalBrawlerChanged;
            }
        }
    }
}
