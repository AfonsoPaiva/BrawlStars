using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Assets.Scripts.Models
{
    public class BrawlerHPDeadState : BrawlerHPBaseState
    {
        public BrawlerHPDeadState(Brawler.BrawlerHPFSM fsm) : base(fsm) { }

        public override void OnEnter()
        {
        }

        public override void FixedUpdate(float fixedDeltaTime)
        {
            // Do nothing yet
        }
    }
}