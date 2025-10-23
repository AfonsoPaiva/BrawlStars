using Assets.Scripts.BaseFSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Models
{
    public abstract class BrawlerHPBaseState : IState
    {
        protected Brawler.BrawlerHPFSM FSM { get; }
        protected Brawler Context => FSM.Context;

        public BrawlerHPBaseState(Brawler.BrawlerHPFSM fsm)
        {
            FSM = fsm;
        }

        public virtual void OnEnter() { }
        public virtual void OnExit() { }
        public abstract void FixedUpdate(float fixedDeltaTime);
    }
}