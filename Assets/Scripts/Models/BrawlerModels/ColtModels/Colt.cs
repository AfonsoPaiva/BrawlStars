using Assets.Scripts.Models;
using PD3HealthBars;
using System;
using UnityEngine;

namespace Assets.Scripts.Models.ColtModels
{
    public class Colt : Brawler
    {
        // Events
        public event EventHandler<ColtBulletEventArgs> ColtFired;

        // Private fields
        private bool _isAttacking = false;
        public BrawlerHPFSM HPFSM { get; private set; }

        // Constructor
        public Colt()
        {
            Health = 10;
            HPFSM = new ColtHPFSM(this);
           
        }

        // Raise HealthChanged when Health property changes
        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            // No need to raise HealthChanged here, Brawler already does it
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            HPFSM?.FixedUpdate(Time.fixedDeltaTime);
        }

        // Primary Attack Implementation
        public override void PARequested()
        {
            if (_isAttacking)
            {
                return;
            }

            _isAttacking = true;

            // Fire bullet
            ColtBullet bullet = new ColtBullet();
            ColtFired?.Invoke(this, new ColtBulletEventArgs(bullet));

            // Reset attack flag (in real implementation, would be controlled by attack FSM)
            _isAttacking = false;
        }
    }

    // EventArgs for ColtFired event
    public class ColtBulletEventArgs : EventArgs
    {
        public ColtBullet Bullet { get; }

        public ColtBulletEventArgs(ColtBullet bullet)
        {
            Bullet = bullet;
        }
    }
}