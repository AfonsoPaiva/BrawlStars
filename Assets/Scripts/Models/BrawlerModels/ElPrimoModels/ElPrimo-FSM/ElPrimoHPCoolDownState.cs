using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Models
{
    public class ElPrimoHPCoolDownState : BrawlerHPCoolDownState
    {
        public ElPrimoHPCoolDownState(Brawler.BrawlerHPFSM fsm) : base(fsm)
        {
        }

        // Override if Colt needs different cooldown behavior
    }
}
