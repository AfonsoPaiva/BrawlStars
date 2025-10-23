using GridEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GridEditor
{
    public  class Prop : MonoBehaviour
    {
        [HideInInspector] public BuildArea buildArea;
        public float YOffset = 0;

        private void OnDrawGizmos()
        {
            if (buildArea != null && buildArea.showTileTopGizmos)
            {
                Vector3 startPos = transform.position - new Vector3(0, YOffset, 0);
                Gizmos.DrawSphere(startPos, 0.025f);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(startPos, startPos + new Vector3(0, -0.5f, 0));

                Gizmos.DrawLine(startPos, startPos + new Vector3(0, 0, 0.5f));
                Gizmos.DrawLine(startPos, startPos + new Vector3(0, 0, -0.5f));
                Gizmos.DrawLine(startPos, startPos + new Vector3(0.5f, 0, 0));
                Gizmos.DrawLine(startPos, startPos + new Vector3(-0.5f, 0, 0));
            }
        }
    }
}
