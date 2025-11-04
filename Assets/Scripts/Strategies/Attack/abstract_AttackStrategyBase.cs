namespace Assets.Scripts.Strategies.Attack
{
    public abstract class AttackStrategyBase : IAttackStrategy
    {
        public abstract void Execute(float deltaTime);
        public abstract bool CanExecute();
    }
}