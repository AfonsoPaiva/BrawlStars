//tutorial: https://www.youtube.com/watch?v=21a0tMRQsFs

using System.Diagnostics;
using UnityEngine;

namespace GridEditor
{
    public class BuildArea : MonoBehaviour
    {
        [HideInInspector] public float tileSize;
        [HideInInspector] public Vector2 dimension;

        public bool showBuildAreaGizmo = true;
        public bool showTileTopGizmos = true;
        public bool showPropBottomGizmos = true;

        public void BuildNodes()
        {
            for (int x = 0; x < dimension.x; x++)
                for (int z = 0; z < dimension.y; z++)
                {
                    Vector3 pos = new Vector3(transform.position.x + x * tileSize, transform.position.y, transform.position.z + z * tileSize);
                    GameObject node = new GameObject();
                    node.transform.position = pos;
                    node.transform.parent = transform;
                    node.name = $"Node ({x},{z})";
                    Node nodescript = node.AddComponent<Node>();
                    nodescript.buildArea = this;
                    UnityEditor.EditorUtility.SetDirty(node);
                }
        }

        private void OnDrawGizmos()
        {
            if (!showBuildAreaGizmo) return;
            for (int x = 0; x < dimension.x; x++)
                for (int z = 0; z < dimension.y; z++)
                {
                    Vector3 pos = new Vector3(transform.position.x + x * tileSize, transform.position.y, transform.position.z + z * tileSize);
                    Gizmos.DrawWireCube(pos, new Vector3(tileSize, 0, tileSize));
                }
        }
    }
}
