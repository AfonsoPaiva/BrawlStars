using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.FSM;
using Assets.Scripts.Interfaces;

namespace Assets.Scripts.Models
{
    public partial class Brawler
    {
        public class BrawlerHPFSM : FiniteStateMachine
        {
            public Brawler Context { get; }

            // States
            public BrawlerHPBaseState RegeneratingState { get; set; }
            public BrawlerHPBaseState CoolDownState { get; set; }
            public BrawlerHPBaseState DeadState { get; set; }

            // Hide base CurrentState with more specific type
            public new BrawlerHPBaseState CurrentState
            {
                get => (BrawlerHPBaseState)base.CurrentState;
                set => base.CurrentState = value;
            }

            public BrawlerHPFSM(Brawler context)
            {
                Context = context;
            }
        }
    }
}