using Assets.Scripts.Models;
using UnityEngine;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Strategies;

namespace Assets.Scripts.Presenters
{
    public class ElPrimopresenter : BrawlerPresenter
    {
        [Header("ElPrimo Settings")]
        [SerializeField] private float elPrimoRotationSpeed = 180f;
        [SerializeField] private float automatedAttackInterval = 1.0f;

        public float ElPrimoRotationSpeed => elPrimoRotationSpeed;
        public float AutomatedAttackInterval => automatedAttackInterval;

        protected override void Awake()
        {
            base.Awake();
            Model = new ElPrimo();
        }

     
        protected override IMovementStrategy CreateNonLocalMovementStrategy()
        {
            return new RotationalMovementStrategy(elPrimoRotationSpeed);
        }

        protected override IAttackStrategy CreateNonLocalAttackStrategy()
        {
            return new AutomatedAttackStrategy(automatedAttackInterval);
        }

        protected override void ModelSetInitialization(Brawler previousModel)
        {
            base.ModelSetInitialization(previousModel);
        }
    }
}