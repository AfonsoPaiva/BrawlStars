using System;

namespace PD3HealthBars {
    public interface IHealthBar {
        public event EventHandler HealthChanged;
        public float HealthProgress { get; }    // range [0,1]
    }
}