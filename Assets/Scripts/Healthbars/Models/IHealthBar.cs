using System;

namespace Assets.Scripts.Healthbars{
    public interface IHealthBar {
        public event EventHandler HealthChanged;
        public float HealthProgress { get; }    // range [0,1]
    }
}