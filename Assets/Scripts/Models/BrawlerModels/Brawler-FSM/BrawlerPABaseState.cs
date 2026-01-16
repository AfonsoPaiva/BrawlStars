using Assets.Scripts.FSM;
using Assets.Scripts.Interfaces;

namespace Assets.Scripts.Models
{
    public abstract class BrawlerPABaseState : IState
    {
        protected Brawler.BrawlerPAFSM FSM { get; }
        protected Brawler Context => FSM.Context;

        public BrawlerPABaseState(Brawler.BrawlerPAFSM fsm)
        {
            FSM = fsm;
        }

        public virtual void OnEnter() { }
        public virtual void OnExit() { }
        public abstract void FixedUpdate(float fixedDeltaTime);
    }
}