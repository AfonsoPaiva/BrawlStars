using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Interfaces;

namespace Assets.Scripts.FSM
{
    public abstract class FiniteStateMachine
    {
        public event EventHandler CurrentStateChanged;

        private IState _currentState;

        public IState CurrentState
        {
            get => _currentState;
            protected set
            {
                if (_currentState != value)
                {
                    _currentState?.OnExit();
                    _currentState = value;
                    _currentState?.OnEnter();
                    CurrentStateChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public void TransitionTo(IState newState)
        {
            CurrentState = newState;
        }

        public void FixedUpdate(float fixedDeltaTime)
        {
            _currentState?.FixedUpdate(fixedDeltaTime);
        }
    }
}
