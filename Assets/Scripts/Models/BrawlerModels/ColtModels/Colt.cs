using Assets.Scripts.Models;
using NUnit.Framework;
using PD3HealthBars;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Models.ColtModels
{
    public class Colt : Brawler
    {
        // Events
        public event EventHandler<ColtBulletEventArgs> ColtFired;

        // Collections for the pool implementation
        public const int TOTAL_BULLETS_SIZE = 20;
        private readonly List<ColtBullet> _bulletPool;
        public IReadOnlyList<ColtBullet> BulletPool => _bulletPool.AsReadOnly();

        // Track next available bullet index for round-robin pool access
        private int _nextBulletIndex = 0;

        // Private fields
        private bool _isAttacking = false;
        public BrawlerHPFSM HPFSM { get; private set; }

        // Constructor
        public Colt()
        {
            Health = 10;
            HPFSM = new ColtHPFSM(this);

            // initialize pool 
            _bulletPool = new List<ColtBullet>(TOTAL_BULLETS_SIZE);
            for (int i = 0; i < TOTAL_BULLETS_SIZE; i++)
            {
                var b = new ColtBullet();
                // Model owns lifecycle: listen for Expired to release into pool
                b.Expired += OnBulletExpired;
                _bulletPool.Add(b);
            }
        }

        private void OnBulletExpired(object sender, EventArgs e)
        {
            if (sender is ColtBullet bullet && _bulletPool.Contains(bullet))
            {
                ReleaseBullet(bullet);
            }
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            HPFSM?.FixedUpdate(Time.fixedDeltaTime);
        }

        public ColtBullet TryAcquireBullet()
        {
            // Search through the entire pool for an inactive bullet
            for (int i = 0; i < TOTAL_BULLETS_SIZE; i++)
            {
                var bullet = _bulletPool[_nextBulletIndex];
                
                // Only return if this bullet is inactive (available)
                if (!bullet.IsActive)
                {
                    // Move to next index for next acquisition attempt
                    _nextBulletIndex++;
                    if (_nextBulletIndex >= TOTAL_BULLETS_SIZE)
                    {
                        _nextBulletIndex = 0;
                    }
                    return bullet;
                }
                
                // Try next bullet in pool
                _nextBulletIndex++;
                if (_nextBulletIndex >= TOTAL_BULLETS_SIZE)
                {
                    _nextBulletIndex = 0;
                }
            }
            
            // No available bullets in pool - return null
            return null;
        }

        public void ReleaseBullet(ColtBullet bullet)
        {
            if (bullet == null)
            {
                return;
            }

            bullet.ResetForPool();
        }

        // Primary Attack Implementation
        public override void PARequested()
        {
            if (_isAttacking)
            {
                return;
            }

            _isAttacking = true;

            ColtBullet bullet = TryAcquireBullet();
            
            // Only fire if a bullet was successfully acquired from the pool
            if (bullet != null)
            {
                //for the presenter to see where to spawn the bullet
                ColtFired?.Invoke(this, new ColtBulletEventArgs(bullet));
            }

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