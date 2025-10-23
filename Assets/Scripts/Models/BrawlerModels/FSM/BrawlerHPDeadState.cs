using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Models
{
    public class BrawlerHPDeadState : BrawlerHPBaseState
    {
        public BrawlerHPDeadState(Brawler.BrawlerHPFSM fsm) : base(fsm) { }

        public override void OnEnter()
        {
            UnityEngine.Debug.Log($"{Context.GetType().Name} is dead!");
        }

        public override void FixedUpdate(float fixedDeltaTime)
        {
            // Do nothing yet
        }
    }
}