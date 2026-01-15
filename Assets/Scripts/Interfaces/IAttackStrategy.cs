namespace Assets.Scripts.Interfaces
{
    public interface IAttackStrategy
    {
        void Execute(float deltaTime);
        bool CanExecute();
    }
}