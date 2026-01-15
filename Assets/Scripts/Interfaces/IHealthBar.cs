using System;

namespace Assets.Scripts.Interfaces
{
    public interface IHealthBar
    {
        event EventHandler HealthChanged;
        float HealthProgress { get; } 
    }
}
