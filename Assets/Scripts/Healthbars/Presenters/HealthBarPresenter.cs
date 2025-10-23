using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace PD3HealthBars {
    public class HealthBarPresenter
    {
        const float _width = 40f;
        const float _height = 10f;

        public IHealthBar Model { get; set; }
        public VisualElement HBClone { get; set; }

        private readonly Transform _hbTransform;
        private readonly UIDocument _hudDocument;
        private VisualElement _blackSide;

        public HealthBarPresenter(IHealthBar healthPublisher, Transform hbTransform, VisualElement hbClone, UIDocument hudDocument)
        {
            Model = healthPublisher;
            Model.HealthChanged += Model_HealthChanged;

            _hbTransform = hbTransform;
            HBClone = hbClone;
            _hudDocument = hudDocument;
            _hudDocument.rootVisualElement.Add(hbClone);
            _blackSide = hbClone.Q("BlackSide");
            _blackSide.style.width = new StyleLength(new Length(100 - Model.HealthProgress * 100, LengthUnit.Percent));

            hbClone.style.position = Position.Absolute;
            hbClone.style.width = _width;
            hbClone.style.height = _height / 2;
        }

        private void Model_HealthChanged(object sender, EventArgs e)
        {
            _blackSide.style.width = new StyleLength(new Length(100 - Model.HealthProgress * 100, LengthUnit.Percent));
        }

        public void UpdatePosition()
        {
            float dist = Vector3.Distance(Camera.main.transform.position, _hbTransform.position);
            float distScale = 1f / Mathf.Clamp01(dist / 20f);

            Vector2 screenpos = RuntimePanelUtils.CameraTransformWorldToPanel(_hudDocument.runtimePanel, _hbTransform.position, Camera.main);

            HBClone.style.top = screenpos.y; // -_height*distScale;
            HBClone.style.left = screenpos.x - _width * distScale / 2;
            HBClone.style.width = _width * distScale;
            HBClone.style.height = _height * distScale;
        }
    }
}

