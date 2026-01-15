using Assets.Scripts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Models
{
    public class ColtHPFSM : Brawler.BrawlerHPFSM
    {
        public ColtHPFSM(Colt context) : base(context)
        {
            // Initialize states
            RegeneratingState = new BrawlerHPRegeneratingState(this);
            CoolDownState = new BrawlerHPCoolDownState(this);
            DeadState = new BrawlerHPDeadState(this);

            // Set initial state
            CurrentState = RegeneratingState;
        }
    }
}