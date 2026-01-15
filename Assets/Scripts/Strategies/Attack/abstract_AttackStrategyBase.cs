
using Assets.Scripts.Interfaces;

namespace Assets.Scripts.Strategies
{
    public abstract class AttackStrategyBase : IAttackStrategy
    {
        public abstract void Execute(float deltaTime);
        public abstract bool CanExecute();
        public virtual void UpdateCooldown(float deltaTime) { }
        public virtual void Cleanup() { }
    }
}