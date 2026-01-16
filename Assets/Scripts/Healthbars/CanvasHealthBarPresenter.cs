using UnityEngine;
using UnityEngine.UI;
using System;
using IHealthBar = Assets.Scripts.Interfaces.IHealthBar;

namespace Assets.Scripts.Healthbars
{
    public class CanvasHealthBarPresenter : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image healthBarFill;
        [SerializeField] private Image healthBarBackground;

        private IHealthBar _model; // This now uses Assets.Scripts.Interfaces.IHealthBar

        /// <summary>
        /// Configure this presenter with a model
        /// </summary>
        public void Configure(IHealthBar model)
        {
            // Unsubscribe from previous model
            if (_model != null)
            {
                _model.HealthChanged -= OnHealthChanged;
            }

            _model = model;

            // Subscribe to new model
            if (_model != null)
            {
                _model.HealthChanged += OnHealthChanged;
                UpdateHealthBar();
            }
        }

        private void OnHealthChanged(object sender, EventArgs e)
        {
            UpdateHealthBar();
        }

        private void UpdateHealthBar()
        {
            if (_model != null && healthBarFill != null)
            {
                healthBarFill.fillAmount = _model.HealthProgress;
            }
        }

        private void OnDestroy()
        {
            if (_model != null)
            {
                _model.HealthChanged -= OnHealthChanged;
            }
        }
    }
}