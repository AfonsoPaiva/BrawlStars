using Assets.Scripts.Models;
using Assets.Scripts.Models.ColtModels;
using UnityEngine;

namespace Assets.Scripts.Strategies.Damage
{
    public interface IDamageStrategy
    {
        float CalculateDamage(Brawler target, Vector3 targetPosition, ColtBullet bullet);
        void ApplyDamage(Brawler target, Vector3 targetPosition, ColtBullet bullet);
    }
}
