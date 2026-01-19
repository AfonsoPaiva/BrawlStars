using System.ComponentModel;

namespace Assets.Scripts.Models
{
    public class BrawlerPAReadyState : BrawlerPABaseState
    {
        private float _lastHealth;

        public BrawlerPAReadyState(Brawler.BrawlerPAFSM fsm) : base(fsm) { }

        public override void OnEnter()
        {
            Context.PropertyChanged += Context_PropertyChanged;
            _lastHealth = Context.Health;

            // Set PA progress to 100% when ready
            Context.PAProgress = 1.0f;

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
            // Check if brawler took damage - PA remains ready
            else if (Context.Health < _lastHealth)
            {
                _lastHealth = Context.Health;
                // PA stays ready even when taking damage
            }
            else
            {
                _lastHealth = Context.Health;
            }
        }

        public override void FixedUpdate(float fixedDeltaTime)
        {
            // PA is ready - maintain 100% progress
            Context.PAProgress = 1.0f;
        }
    }
}