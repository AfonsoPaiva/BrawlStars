using Assets.Scripts.BaseFSM;
using PD3HealthBars;
using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Models
{
    public abstract partial class Brawler : UnityModelBaseClass, IHealthBar
    {
        // Constants
        public const float MAXHEALTH = 100f;
        public const float HEALTH_REGEN_PERCENT = 0.13f; // 13% per second
        public const float COOLDOWN_DURATION = 3f;

        // Private fields
        private float _health;
        public float Health
        {
            get => _health;
            set
            {
                if (_health != value)
                {
                    _health = Mathf.Clamp(value, 0, MAXHEALTH);
                    OnPropertyChanged(nameof(Health));
                    HealthChanged?.Invoke(this, EventArgs.Empty);
                    
                    // Check for death
                    if (_health <= 0)
                    {
                        OnDeath();
                    }
                }
            }
        }


        public event EventHandler HealthChanged;
        public event EventHandler Died;
        public float HealthProgress => Mathf.Clamp01(_health / MAXHEALTH);

        // Health bar presenter field
        protected HealthBarPresenter _healthBarPresenter;

        public abstract void PARequested();

        public void HPRegenerate(float deltaTime)
        {
            if (Health < MAXHEALTH)
            {
                Health = Mathf.Min(MAXHEALTH, Health + (MAXHEALTH * HEALTH_REGEN_PERCENT * deltaTime));
            }
        }

        // Damage method
        public void TakeDamage(float damage)
        {
            Health -= damage;
        }

        // Death notification method
        protected virtual void OnDeath()
        {
            Died?.Invoke(this, EventArgs.Empty);
        }

        // Override FixedUpdate to delegate to FSM
        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }
    }
}