using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.BaseFSM
{
    public interface IState
    {
        void OnEnter();
        void OnExit();
        void FixedUpdate(float fixedDeltaTime);
    }
}
