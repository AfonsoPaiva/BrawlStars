using Assets.Scripts.Common;

namespace Assets.Scripts.Interfaces
{
    public interface IDamageStrategy
    {
        float CalculateDamage(IBrawler target, SerializableVector3 targetPosition, IBullet bullet);
        void ApplyDamage(IBrawler target, SerializableVector3 targetPosition, IBullet bullet);
    }
}
