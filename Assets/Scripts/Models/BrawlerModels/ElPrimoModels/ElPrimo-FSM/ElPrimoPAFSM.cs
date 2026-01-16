namespace Assets.Scripts.Models
{
    public class ElPrimoPAFSM : Brawler.BrawlerPAFSM
    {
        public ElPrimoPAFSM(ElPrimo context) : base(context)
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