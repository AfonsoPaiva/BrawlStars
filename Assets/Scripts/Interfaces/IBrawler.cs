using System;

namespace Assets.Scripts.Interfaces
{
    public interface IBrawler
    {
        void TakeDamage(float damage);
        float Health { get; }
        event EventHandler HealthChanged;
    }
}
