using System;

namespace Assets.Scripts.Interfaces
{
    /// <summary>
    /// Generic animation interface that doesn't depend on UnityEngine
    /// Allows Model layer to observe animations without Unity dependencies
    /// </summary>
    /// <typeparam name="T">The type of value being animated (e.g., SerializableVector3)</typeparam>
    public interface IAnimation<T>
    {
        /// <summary>
        /// Raised when the animation completes
        /// </summary>
        event EventHandler AnimationEnded;

        /// <summary>
        /// Raised each frame with the current interpolated value
        /// </summary>
        event EventHandler<AnimationValueChangedEventArgs<T>> ValueChanged;

        /// <summary>
        /// Current progress of the animation (0-1)
        /// </summary>
        float Progress { get; }

        /// <summary>
        /// Whether the animation is currently playing
        /// </summary>
        bool IsPlaying { get; }

        /// <summary>
        /// Start the animation
        /// </summary>
        void Play();

        /// <summary>
        /// Stop the animation
        /// </summary>
        void Stop();

        /// <summary>
        /// Update the animation (called each frame)
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        void Update(float deltaTime);
    }

    /// <summary>
    /// Event args for ValueChanged event
    /// </summary>
    public class AnimationValueChangedEventArgs<T> : EventArgs
    {
        public T CurrentValue { get; }
        public float Progress { get; }

        public AnimationValueChangedEventArgs(T currentValue, float progress)
        {
            CurrentValue = currentValue;
            Progress = progress;
        }
    }
}