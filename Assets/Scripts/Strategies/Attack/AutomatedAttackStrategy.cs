namespace Assets.Scripts.Strategies.Attack
{
    public class AutomatedAttackStrategy : AttackStrategyBase
    {
        private float _attackCooldown;
        private readonly float _attackInterval;

        public AutomatedAttackStrategy(float attackInterval = 1.0f)
        {
            _attackInterval = attackInterval;
            _attackCooldown = 0f;
        }

        public override bool CanExecute()
        {
            return _attackCooldown <= 0f;
        }

        public override void Execute(float deltaTime)
        {
            _attackCooldown -= deltaTime;

            if (_attackCooldown < 0f)
            {
                _attackCooldown = 0f;
            }
        }

        public void ResetCooldown()
        {
            _attackCooldown = _attackInterval;
        }
    }
}