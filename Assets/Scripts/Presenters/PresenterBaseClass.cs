using System.ComponentModel;
using UnityEngine;

namespace Assets.Scripts.Presenters
{
    public abstract class PresenterBaseClass<T> : MonoBehaviour where T : UnityModelBaseClass
    {
        private T _model;

        public T Model
        {
            get => _model;
            set
            {
                // Unsubscribe from old model
                if (_model != null)
                {
                    _model.PropertyChanged -= Model_PropertyChanged;
                }

                T previousModel = _model;
                _model = value;

                // Subscribe to new model
                if (_model != null)
                {
                    _model.PropertyChanged += Model_PropertyChanged;
                    ModelSetInitialization(previousModel);
                }
            }
        }

        protected virtual void Awake() { }

        protected virtual void OnDestroy() { }

        protected virtual void ModelSetInitialization(T previousModel) { }

        protected abstract void Model_PropertyChanged(object sender, PropertyChangedEventArgs e);

        protected virtual void Update()
        {
            Model?.Update();
        }

        protected virtual void FixedUpdate()
        {
            Model?.FixedUpdate();
        }
    }
}