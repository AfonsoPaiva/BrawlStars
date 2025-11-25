using Assets.Scripts.Models;
using Assets.Scripts.Models.ElPrimoModels;
using Assets.Scripts.Strategies.Attack;
using Assets.Scripts.Strategies.Movement;
using UnityEngine;

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