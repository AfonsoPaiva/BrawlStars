using Assets.Scripts.Models;
using UnityEngine;

namespace Assets.Scripts.Strategies.Damage
{
    public interface IDamageStrategy
    {
        void ApplyDamage(Brawler target, float baseDamage, GameObject sourceObject, GameObject targetObject);
        float CalculateDamage(float baseDamage, Brawler target, GameObject sourceObject, GameObject targetObject);
    }
}
