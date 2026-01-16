using System;
using UnityEngine;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;

namespace Assets.Scripts.Presenters
{
    /// <summary>
    /// Concrete animation implementation in the Presenter layer
    /// Animates Unity Vector3 but exposes Model-friendly SerializableVector3
    /// </summary>
    public class Vector3Animation : IAnimation<SerializableVector3>
    {
        public event EventHandler AnimationEnded;
        public event EventHandler<AnimationValueChangedEventArgs<SerializableVector3>> ValueChanged;

        private readonly Vector3 _startValue;
        private readonly Vector3 _endValue;
        private readonly float _duration;
        private readonly AnimationCurve _curve;

        private float _currentTime;
        private bool _isPlaying;

        public float Progress => _duration > 0 ? Mathf.Clamp01(_currentTime / _duration) : 1f;
        public bool IsPlaying => _isPlaying;

        public Vector3 CurrentUnityValue { get; private set; }

        public Vector3Animation(Vector3 startValue, Vector3 endValue, float duration, AnimationCurve curve = null)
        {
            _startValue = startValue;
            _endValue = endValue;
            _duration = duration;
            _curve = curve ?? AnimationCurve.Linear(0, 0, 1, 1);
            CurrentUnityValue = startValue;
        }

        public void Play()
        {
            _isPlaying = true;
            _currentTime = 0f;
            CurrentUnityValue = _startValue;
        }

        public void Stop()
        {
            _isPlaying = false;
        }

        public void Update(float deltaTime)
        {
            if (!_isPlaying) return;

            _currentTime += deltaTime;

            float t = _curve.Evaluate(Progress);
            CurrentUnityValue = Vector3.Lerp(_startValue, _endValue, t);

            // Convert to SerializableVector3 for Model layer
            var serializableValue = new SerializableVector3(
                CurrentUnityValue.x,
                CurrentUnityValue.y,
                CurrentUnityValue.z
            );

            // Raise ValueChanged event
            ValueChanged?.Invoke(this, new AnimationValueChangedEventArgs<SerializableVector3>(serializableValue, Progress));

            // Check if animation completed
            if (_currentTime >= _duration)
            {
                _isPlaying = false;
                AnimationEnded?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}