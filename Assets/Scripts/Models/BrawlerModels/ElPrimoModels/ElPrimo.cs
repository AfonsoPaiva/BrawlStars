using Assets.Scripts.Models;
using System;
using UnityEngine;

namespace Assets.Scripts.Models
{
    public class ElPrimo : Brawler
    {
        // Events
        public event EventHandler<ElPrimoBulletEventArgs> ElPrimoFired;

        // Private fields
        private bool _isAttacking = false;
        public BrawlerHPFSM HPFSM { get; private set; }

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
            // No need to raise HealthChanged here, Brawler already does it
        }

        public override void PARequested()
        {
            if (_isAttacking)
            {
                return;
            }

            _isAttacking = true;

            // Fire bullet
            ElPrimoBullet bullet = new ElPrimoBullet();
            ElPrimoFired?.Invoke(this, new ElPrimoBulletEventArgs(bullet));

            // Reset attack flag
            _isAttacking = false;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            HPFSM?.FixedUpdate(Time.fixedDeltaTime);
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
}