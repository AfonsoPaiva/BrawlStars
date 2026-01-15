using Assets.Scripts.Models;
using System.ComponentModel;
using UnityEngine;

namespace Assets.Scripts.Presenters
{
    [RequireComponent(typeof(Collider))]
    public class ElPrimoBulletPresenter : PresenterBaseClass<ElPrimoBullet>
    {
        protected override void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }
    }
}