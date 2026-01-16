using System.ComponentModel;
using UnityEngine;

namespace Assets.Scripts.Models
{
    public class BrawlerPACooldownState : BrawlerPABaseState
    {
        // Use a constant from Brawler or define a specific PA cooldown
        public const float PA_COOLDOWN_DURATION = 0.2f; // Configurable attack cooldown

        private float _cooldownTimer;
        private float _lastHealth;

        public BrawlerPACooldownState(Brawler.BrawlerPAFSM fsm) : base(fsm) { }

        public override void OnEnter()
        {
            _cooldownTimer = 0f;
            _lastHealth = Context.Health;
            Context.PropertyChanged += Context_PropertyChanged;

            Debug.Log($"{Context.GetType().Name} PA on cooldown");
        }

        public override void OnExit()
        {
            Context.PropertyChanged -= Context_PropertyChanged;
        }

        private void Context_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Brawler.Health))
            {
                CheckForChange();
            }
        }

        private void CheckForChange()
        {
            // Check if brawler died - disable attacks
            if (Context.Health <= 0)
            {
                FSM.TransitionTo(FSM.DisabledState);
            }
            // Check if brawler took damage - cooldown continues
            else if (Context.Health < _lastHealth)
            {
                _lastHealth = Context.Health;
                // Cooldown is not affected by taking damage
            }
            else
            {
                _lastHealth = Context.Health;
            }
        }

        public override void FixedUpdate(float fixedDeltaTime)
        {
            _cooldownTimer += fixedDeltaTime;

            // Update PA progress based on cooldown
            Context.PAProgress = Mathf.Clamp01(_cooldownTimer / PA_COOLDOWN_DURATION);

            // After cooldown duration, transition to ready
            if (_cooldownTimer >= PA_COOLDOWN_DURATION)
            {
                FSM.TransitionTo(FSM.ReadyState);
            }
        }
    }
}