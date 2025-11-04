using Assets.Scripts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Models.ElPrimoModels
{
    public class ElPrimoHPFSM : Brawler.BrawlerHPFSM
    {
        public ElPrimoHPFSM(ElPrimo context) : base(context)
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