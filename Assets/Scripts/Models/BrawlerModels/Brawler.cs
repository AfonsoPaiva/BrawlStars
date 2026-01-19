using System;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Common;

namespace Assets.Scripts.Models
{
    public abstract partial class Brawler : UnityModelBaseClass, IBrawler, IHealthBar, IHUD
    {
        // Constants
        public const float MAXHEALTH = 100f;
        public const float HEALTH_REGEN_PERCENT = 0.13f; // 13% per second
        public const float COOLDOWN_DURATION = 3f;

        // --- NEW: Damage Strategy Support ---
        private IDamageStrategy _damageStrategy;

        public void SetDamageStrategy(IDamageStrategy strategy)
        {
            _damageStrategy = strategy;
        }

        // Optional: Expose logic to calculate damage using the stored strategy
        public float CalculateDamage(IBrawler target, SerializableVector3 position, IBullet bullet)
        {
            if (_damageStrategy != null)
            {
                return _damageStrategy.CalculateDamage(target, position, bullet);
            }
            return bullet?.Damage ?? 0f; // Default fallback
        }
        // ------------------------------------

        // Private fields
        private float _health;
        public float Health
        {
            get => _health;
            set
            {
                if (_health != value)
                {
                    _health = MathHelper.Clamp(value, 0, MAXHEALTH);
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
        public float HealthProgress => MathHelper.Clamp01(_health / MAXHEALTH);

        // HUD: PA progress and display name
        private float _paProgress;
        public float PAProgress
        {
            get => _paProgress;
            set
            {
                float clamped = MathHelper.Clamp01(value);
                if (!MathHelper.Approximately(_paProgress, clamped))
                {
                    _paProgress = clamped;
                    OnPropertyChanged(nameof(PAProgress));
                    PAProgressChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler PAProgressChanged;

        public string DisplayName => GetType().Name;
        // End HUD additions

        public abstract void PARequested();

        public void HPRegenerate(float deltaTime)
        {
            if (Health < MAXHEALTH)
            {
                Health = MathHelper.Min(MAXHEALTH, Health + (MAXHEALTH * HEALTH_REGEN_PERCENT * deltaTime));
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