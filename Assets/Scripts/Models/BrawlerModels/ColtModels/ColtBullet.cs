using System;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Strategies;
using Assets.Scripts.Common;

namespace Assets.Scripts.Models
{
    public class ColtBullet : UnityModelBaseClass, IBullet
    {
        public event EventHandler Expired;
        public event EventHandler PositionChanged;

        public const float BULLET_SPEED = 15f;
        public const float TIME_TO_LIVE = 3f;
        public const float BASE_DAMAGE = 10f;

        private float _timeToLive;
        private SerializableVector3 _position;
        private SerializableVector3 _direction;
        private float _speed;
        private float _damage;
        private Brawler _owner;
        private IDamageStrategy _damageStrategy;

        public IDamageStrategy DamageStrategy
        {
            get => _damageStrategy ??= new StandardDamageStrategy();
            set => _damageStrategy = value ?? new StandardDamageStrategy();
        }

        //this is for the pool implementation
        public bool IsActive { get; internal set; } = false;

        public float TimeToLive
        {
            get => _timeToLive;
            private set
            {
                if (_timeToLive != value)
                {
                    _timeToLive = value;
                    OnPropertyChanged();
                }
            }
        }

        public SerializableVector3 Position
        {
            get => _position;
            private set
            {
                if (_position.X != value.X || _position.Y != value.Y || _position.Z != value.Z)
                {
                    _position = value;
                    PositionChanged?.Invoke(this, EventArgs.Empty);
                    OnPropertyChanged();
                }
            }
        }

        public SerializableVector3 Direction
        {
            get => _direction;
            set => _direction = value.Normalized;
        }

        public float Speed
        {
            get => _speed;
            set => _speed = value;
        }

        public float Damage
        {
            get => _damage;
            set => _damage = value;
        }

        public Brawler Owner
        {
            get => _owner;
            set => _owner = value;
        }

        //  initialize bullet 
        public void Initialize(SerializableVector3 startPosition, SerializableVector3 direction, Brawler owner = null)
        {
            Position = startPosition;
            Direction = direction;
            Speed = BULLET_SPEED;
            TimeToLive = TIME_TO_LIVE;
            Damage = BASE_DAMAGE;
            Owner = owner;
            IsActive = true;
            OnPropertyChanged(nameof(IsActive));
        }

        public void MarkExpiredByHit()
        {
            if (!IsActive)
            {
                return;
            }

            // Make inactive first to avoid re-entry from Update
            IsActive = false;
            Expired?.Invoke(this, EventArgs.Empty);
        }

        public void ResetForPool()
        {
            IsActive = false;
            // this is for the presenters to know it's inactive
            TimeToLive = 0f;
            _position = SerializableVector3.Zero;
            _direction = SerializableVector3.Zero;
            Speed = 0f;
            Damage = 0f;
            Owner = null;
            // Reset to default strategy when returning to pool
            _damageStrategy = new StandardDamageStrategy();
            OnPropertyChanged(nameof(IsActive));
        }

        public override void Update()
        {
            base.Update();

            if (!IsActive)
            {
                return;
            }

            if (TimeToLive > 0)
            {
                SerializableVector3 oldPosition = Position;
                Position = Position + Direction * Speed * GameTime.DeltaTime;

                TimeToLive -= GameTime.DeltaTime;

                if (TimeToLive <= 0)
                {
                    // thus is to avoid re-entrancy issues
                    IsActive = false;
                    Expired?.Invoke(this, EventArgs.Empty);
                }
            }
            else
            {
                IsActive = false;
                Expired?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}