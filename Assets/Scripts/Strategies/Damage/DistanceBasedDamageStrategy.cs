using Assets.Scripts.Interfaces;
using Assets.Scripts.Common;

namespace Assets.Scripts.Strategies
{
    public class DistanceBasedDamageStrategy : DamageStrategyBase
    {
        private readonly float _maxRange;
        private readonly float _minDamageMultiplier;

        public DistanceBasedDamageStrategy(float maxRange = 10f, float minDamageMultiplier = 0.5f)
        {
            _maxRange = maxRange;
            _minDamageMultiplier = minDamageMultiplier;
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

            float t = MathHelper.Clamp01(distance / _maxRange);
            float multiplier = MathHelper.Lerp(1f, _minDamageMultiplier, t);
            return bullet.Damage * multiplier;
        }
    }
}