using Assets.Scripts.Models.ColtModels;
using Assets.Scripts.Models.ElPrimoModels;
using Assets.Scripts.Strategies.Damage;
using System.ComponentModel;
using UnityEngine;

namespace Assets.Scripts.Presenters.ElPrimoPresenters
{
    [RequireComponent(typeof(Collider))]
    public class ElPrimoBulletPresenter : PresenterBaseClass<ElPrimoBullet>
    {
        protected override void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }
    }
}