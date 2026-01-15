using System;
using UnityEngine;

namespace Assets.Scripts.Models
{
    public abstract class UnityModelBaseClass : ModelBaseClass
    {
        private static int _nextModelID = 1;
        
        public int ModelID { get; private set; }

        protected UnityModelBaseClass()
        {
            ModelID = _nextModelID++;
        }

        // Allow setting ModelID during replay reconstruction
        public void SetModelID(int id)
        {
            ModelID = id;
        }

        public static void ResetModelIDCounter()
        {
            _nextModelID = 1;
        }

        public virtual void Update()
        {
        }

        public virtual void FixedUpdate()
        {
        }
    }
}
