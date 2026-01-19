using Assets.Scripts.Interfaces;
using Assets.Scripts.Common;

namespace Assets.Scripts.Strategies
{
    public class CriticalDamageStrategy : DamageStrategyBase
    {
        private readonly float _criticalRange;
        private readonly float _criticalMultiplier;

        public CriticalDamageStrategy(float criticalRange = 2f, float criticalMultiplier = 2f)
        {
            _criticalRange = criticalRange;
            _criticalMultiplier = criticalMultiplier;
        }

        public override float CalculateDamage(IBrawler target, SerializableVector3 targetPosition, IBullet bullet)
        {
            if (target == null || bullet == null) return 0f;

            // Calculate distance using SerializableVector3
            SerializableVector3 bulletPos = bullet.Position;
            float dx = bulletPos.X - targetPosition.X;
            float dy = bulletPos.Y - targetPosition.Y;
            float dz = bulletPos.Z - targetPosition.Z;
            float distance = (float)System.Math.Sqrt(dx * dx + dy * dy + dz * dz);

            // Apply critical multiplier if within range
            if (distance <= _criticalRange)
            {
                return bullet.Damage * _criticalMultiplier;
            }

            return bullet.Damage;
        }
    }
}