namespace Assets.Scripts.Strategies.Attack
{
    public class UserInputAttackStrategy : AttackStrategyBase
    {
        private bool _attackRequested;

        public void RequestAttack()
        {
            _attackRequested = true;
        }

        public override bool CanExecute()
        {
            return _attackRequested;
        }

        public override void Execute(float deltaTime)
        {
            // Attack will be triggered from presenter when input is received
            _attackRequested = false;
        }
    }
}