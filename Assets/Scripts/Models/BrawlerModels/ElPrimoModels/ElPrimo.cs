using Assets.Scripts.Models;
using Assets.Scripts.Interfaces;
using System;
using Assets.Scripts.Common;

namespace Assets.Scripts.Models
{
    public class ElPrimo : Brawler
    {
        // Events
        public event EventHandler<ElPrimoBulletEventArgs> ElPrimoFired;
        public event EventHandler<DashRequestedEventArgs> DashRequested;

        // Private fields
        private bool _isAttacking = false;
        private bool _isDashing = false;
        private IAnimation<SerializableVector3> _dashAnimation;

        public BrawlerHPFSM HPFSM { get; private set; }
        public bool IsDashing => _isDashing;

        // Constructor
        public ElPrimo()
        {
            Health = 10;
            HPFSM = new ElPrimoHPFSM(this);
        }

        // Raise HealthChanged when Health property changes
        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Sets the dash animation from the Presenter layer
        /// The Model observes the animation without knowing about Unity
        /// </summary>
        public void SetDashAnimation(IAnimation<SerializableVector3> animation)
        {
            // Unsubscribe from previous animation if any
            if (_dashAnimation != null)
            {
                _dashAnimation.AnimationEnded -= OnDashAnimationEnded;
                _dashAnimation.ValueChanged -= OnDashAnimationValueChanged;
            }

            _dashAnimation = animation;

            // Subscribe to animation events
            if (_dashAnimation != null)
            {
                _dashAnimation.AnimationEnded += OnDashAnimationEnded;
                _dashAnimation.ValueChanged += OnDashAnimationValueChanged;
            }
        }

        /// <summary>
        /// Request a dash attack (called by input/attack strategy)
        /// </summary>
        public void RequestDash(SerializableVector3 dashDirection, float dashDistance)
        {
            if (_isDashing || _isAttacking) return;

            _isDashing = true;

            // Notify presenter to create and start dash animation
            DashRequested?.Invoke(this, new DashRequestedEventArgs(dashDirection, dashDistance));
        }

        /// <summary>
        /// Called when dash animation value changes (empty body as per requirements)
        /// </summary>
        private void OnDashAnimationValueChanged(object sender, AnimationValueChangedEventArgs<SerializableVector3> e)
        {
            // Empty body - movement is handled by Presenter
            // This satisfies the requirement to observe ValueChanged
        }

        /// <summary>
        /// Called when dash animation ends
        /// Transitions attack state machine as per requirements
        /// </summary>
        private void OnDashAnimationEnded(object sender, EventArgs e)
        {
            _isDashing = false;

            // Fire bullet at end of dash
            ElPrimoBullet bullet = new ElPrimoBullet();
            ElPrimoFired?.Invoke(this, new ElPrimoBulletEventArgs(bullet));


        }

        public override void PARequested()
        {
            if (_isAttacking || _isDashing)
            {
                return;
            }

            _isAttacking = true;

            // Standard attack - fire bullet
            ElPrimoBullet bullet = new ElPrimoBullet();
            ElPrimoFired?.Invoke(this, new ElPrimoBulletEventArgs(bullet));

            // Reset attack flag
            _isAttacking = false;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            HPFSM?.FixedUpdate(GameTime.FixedDeltaTime);
        }
    }

    // EventArgs for ElPrimoFired event
    public class ElPrimoBulletEventArgs : EventArgs
    {
        public ElPrimoBullet Bullet { get; }

        public ElPrimoBulletEventArgs(ElPrimoBullet bullet)
        {
            Bullet = bullet;
        }
    }

    // EventArgs for DashRequested event
    public class DashRequestedEventArgs : EventArgs
    {
        public SerializableVector3 Direction { get; }
        public float Distance { get; }

        public DashRequestedEventArgs(SerializableVector3 direction, float distance)
        {
            Direction = direction;
            Distance = distance;
        }
    }
}