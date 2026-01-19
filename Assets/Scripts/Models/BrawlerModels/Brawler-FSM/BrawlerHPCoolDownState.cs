using System;
using System.Collections.Generic;
using System.ComponentModel;
using Assets.Scripts.Models;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Assets.Scripts.Models
{
    public class BrawlerHPCoolDownState : BrawlerHPBaseState
    {
        private float _cooldownTimer;
        private float _lastHealth;

        public BrawlerHPCoolDownState(Brawler.BrawlerHPFSM fsm) : base(fsm) { }

        public override void OnEnter()
        {
            _cooldownTimer = 0f;
            _lastHealth = Context.Health;
            Context.PropertyChanged += Context_PropertyChanged;
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
            // Check if brawler died
            if (Context.Health <= 0)
            {
                FSM.TransitionTo(FSM.DeadState);
            }
            // Check if brawler took damage - reset cooldown timer
            else if (Context.Health < _lastHealth)
            {
                _cooldownTimer = 0f;
                _lastHealth = Context.Health;
            }
            else
            {
                _lastHealth = Context.Health;
            }
        }

        public override void FixedUpdate(float fixedDeltaTime)
        {
            _cooldownTimer += fixedDeltaTime;

            // After cooldown duration, transition to regenerating
            if (_cooldownTimer >= Brawler.COOLDOWN_DURATION)
            {
                FSM.TransitionTo(FSM.RegeneratingState);
            }
        }
    }
}