using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GridEditor
{
    [SelectionBase]
    public class Node : MonoBehaviour
    {
        [HideInInspector] public BuildArea buildArea;
        private void OnDrawGizmos()
        {
            if (buildArea == null) return;

            Gizmos.color = Color.black;
            Gizmos.DrawCube(transform.position, new Vector3(buildArea.tileSize * 0.75f, 0, buildArea.tileSize * 0.75f));
        }
    }
}
