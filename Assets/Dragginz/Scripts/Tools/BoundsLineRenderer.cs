//
// Author  : Oliver Brodhage
// Company : Decentralised Team of Developers
//

using UnityEngine;

namespace Dragginz.Scripts.Tools
{
    public class BoundsLineRenderer : MonoBehaviour
    {
        [SerializeField]
        public LineRenderer lineRenderer;

        [HideInInspector]
        public Bounds bounds;

        [HideInInspector]
        public Vector3 v3PosMin;

        [HideInInspector]
        public Vector3 v3PosMax;

        //
        private void Awake () {

            bounds = new Bounds ();
            if (lineRenderer == null) {
                lineRenderer = gameObject.AddComponent<LineRenderer> ();
            }

            v3PosMin = Vector3.zero;
            v3PosMax = Vector3.zero;
        }

        //
        private void Start () {

            lineRenderer.startWidth = 0.01f;
            lineRenderer.endWidth   = 0.01f;
            lineRenderer.positionCount = 19;
        }

        //
        public void UpdateBounds(Bounds b) {

            bounds = b;
            //Debug.Log ("updateBounds "+b);

            v3PosMin.x = b.center.x - b.extents.x;
            v3PosMin.y = b.center.y - b.extents.y;
            v3PosMin.z = b.center.z - b.extents.z;
            //Debug.Log ("v3PosMin: " + v3PosMin.x.ToString() + ", " + v3PosMin.y.ToString () + ", " + v3PosMin.z.ToString ());

            v3PosMax = v3PosMin + b.size;

            // -z facing
            lineRenderer.SetPosition (0, v3PosMin);
            lineRenderer.SetPosition (1, v3PosMin + new Vector3 (0,        b.size.y, 0));
            lineRenderer.SetPosition (2, v3PosMin + new Vector3 (b.size.x, b.size.y, 0));
            lineRenderer.SetPosition (3, v3PosMin + new Vector3 (b.size.x, 0,        0));
            lineRenderer.SetPosition (4, v3PosMin);

            // -x facing
            lineRenderer.SetPosition (5, v3PosMin + new Vector3 (0, 0,        b.size.z));
            lineRenderer.SetPosition (6, v3PosMin + new Vector3 (0, b.size.y, b.size.z));
            lineRenderer.SetPosition (7, v3PosMin + new Vector3 (0, b.size.y, 0));
            lineRenderer.SetPosition (8, v3PosMin);

            // +z facing
            lineRenderer.SetPosition ( 9, v3PosMin + new Vector3 (0,        0,        b.size.z)); // move to next start point
            lineRenderer.SetPosition (10, v3PosMin + new Vector3 (0,        b.size.y, b.size.z));
            lineRenderer.SetPosition (11, v3PosMin + new Vector3 (b.size.x, b.size.y, b.size.z));
            lineRenderer.SetPosition (12, v3PosMin + new Vector3 (b.size.x, 0,        b.size.z));
            lineRenderer.SetPosition (13, v3PosMin + new Vector3 (0,        0,        b.size.z));

            // +x facing
            lineRenderer.SetPosition (14, v3PosMin + new Vector3 (b.size.x, 0,        b.size.z)); // move to next start point
            lineRenderer.SetPosition (15, v3PosMin + new Vector3 (b.size.x, b.size.y, b.size.z));
            lineRenderer.SetPosition (16, v3PosMin + new Vector3 (b.size.x, b.size.y, 0));
            lineRenderer.SetPosition (17, v3PosMin + new Vector3 (b.size.x, 0,        0));
            lineRenderer.SetPosition (18, v3PosMin + new Vector3 (b.size.x, 0,        b.size.z));
        }
    }
}