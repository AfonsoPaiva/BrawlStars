using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Models
{
    public class BrawlerHPRegeneratingState : BrawlerHPBaseState
    {
        private float _lastHealth;

        public BrawlerHPRegeneratingState(Brawler.BrawlerHPFSM fsm) : base(fsm) { }

        public override void OnEnter()
        {
            Context.PropertyChanged += Context_PropertyChanged;
            _lastHealth = Context.Health;
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
            // Check if brawler took damage (health decreased)
            else if (Context.Health < _lastHealth)
            {
                FSM.TransitionTo(FSM.CoolDownState);
            }
            else
            {
                _lastHealth = Context.Health;
            }
        }

        public override void FixedUpdate(float fixedDeltaTime)
        {
            // Regenerate health
            Context.HPRegenerate(fixedDeltaTime);
        }
    }
}