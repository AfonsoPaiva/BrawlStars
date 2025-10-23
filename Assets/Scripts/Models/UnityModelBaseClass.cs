using Assets.Scripts.Models;
using System;
using System.ComponentModel;
using UnityEngine;

public abstract class UnityModelBaseClass : ModelBaseClass
{
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
}
