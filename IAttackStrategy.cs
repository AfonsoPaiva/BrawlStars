namespace Assets.Scripts.Strategies.Attack
{
    public interface IAttackStrategy
    {
        void Execute(float deltaTime);
        bool CanExecute();
    }
}