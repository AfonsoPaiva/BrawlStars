using System;
using UnityEngine;

namespace Assets.Scripts.Models.ColtModels
{
    public class ColtBullet : UnityModelBaseClass
    {
        public event EventHandler Expired;
        public event EventHandler PositionChanged;

        public const float BULLET_SPEED = 15f;
        public const float TIME_TO_LIVE = 3f;

        private float _timeToLive;
        private Vector3 _position;
        private Vector3 _direction;
        private float _speed;

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

        public Vector3 Position
        {
            get => _position;
            private set
            {
                if (_position != value)
                {
                    _position = value;
                    PositionChanged?.Invoke(this, EventArgs.Empty);
                    OnPropertyChanged();
                }
            }
        }

        public Vector3 Direction
        {
            get => _direction;
            set => _direction = value.normalized;
        }

        public float Speed
        {
            get => _speed;
            set => _speed = value;
        }

        // Public method to initialize bullet (respects encapsulation)
        public void Initialize(Vector3 startPosition, Vector3 direction)
        {
            Position = startPosition;
            Direction = direction;
            Speed = BULLET_SPEED;
            TimeToLive = TIME_TO_LIVE;
        }

        public override void Update()
        {
            base.Update();

            if (TimeToLive > 0)
            {
                Vector3 oldPosition = Position;
                Position += Direction * Speed * Time.deltaTime;

                TimeToLive -= Time.deltaTime;

                if (TimeToLive <= 0)
                {
                    Expired?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}