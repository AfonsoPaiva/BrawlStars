using UnityEngine;

namespace Assets.Scripts.Models
{
    public class BrawlerPADisabledState : BrawlerPABaseState
    {
        public BrawlerPADisabledState(Brawler.BrawlerPAFSM fsm) : base(fsm) { }

        public override void OnEnter()
        {
            Context.PAProgress = 0f;
            Debug.Log($"{Context.GetType().Name} PA is disabled (dead)!");
        }

        public override void FixedUpdate(float fixedDeltaTime)
        {
            // Do nothing - brawler is dead, attacks disabled
            Context.PAProgress = 0f;
        }
    }
}