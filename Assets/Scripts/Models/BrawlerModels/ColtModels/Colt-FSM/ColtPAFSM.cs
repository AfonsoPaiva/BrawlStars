namespace Assets.Scripts.Models
{
    public class ColtPAFSM : Brawler.BrawlerPAFSM
    {
        public ColtPAFSM(Colt context) : base(context)
        {
            // Initialize states
            ReadyState = new BrawlerPAReadyState(this);
            CooldownState = new BrawlerPACooldownState(this);
            DisabledState = new BrawlerPADisabledState(this);

            // Set initial state
            CurrentState = ReadyState;
        }
    }
}