using System;
using Assets.Scripts.FSM;
using Assets.Scripts.Interfaces;

namespace Assets.Scripts.Models
{
    public partial class Brawler
    {
        public class BrawlerPAFSM : FiniteStateMachine
        {
            public Brawler Context { get; }

            // States
            public BrawlerPABaseState ReadyState { get; set; }
            public BrawlerPABaseState CooldownState { get; set; }
            public BrawlerPABaseState DisabledState { get; set; }

            // Hide base CurrentState with more specific type
            public new BrawlerPABaseState CurrentState
            {
                get => (BrawlerPABaseState)base.CurrentState;
                set => base.CurrentState = value;
            }

            public BrawlerPAFSM(Brawler context)
            {
                Context = context;
            }
        }
    }
}