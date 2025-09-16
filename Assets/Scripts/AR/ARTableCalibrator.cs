using UnityEngine;

#if UNITY_XR_ARFOUNDATION
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#endif

namespace BilliardMasterAi.AR
{
    // Places and scales a virtual carom table on a detected plane.
    // Define scripting symbol UNITY_XR_ARFOUNDATION when AR Foundation is installed.
    public class ARTableCalibrator : MonoBehaviour
    {
        public Transform tableRoot; // parent transform of table visuals
        public float tableWidth = BilliardMasterAi.Physics.CaromConstants.TableWidth;
        public float tableHeight = BilliardMasterAi.Physics.CaromConstants.TableHeight;

#if UNITY_XR_ARFOUNDATION
        [SerializeField] private ARRaycastManager _raycaster;
        private static readonly System.Collections.Generic.List<ARRaycastHit> _hits = new();
#endif

        void Awake()
        {
            if (tableRoot == null)
            {
                var go = new GameObject("ARCaromTable");
                tableRoot = go.transform;
            }
        }

        void Update()
        {
            if (Input.touchCount == 0) return;
            var t = Input.GetTouch(0);
            if (t.phase != TouchPhase.Began) return;

#if UNITY_XR_ARFOUNDATION
            if (_raycaster == null) _raycaster = FindObjectOfType<ARRaycastManager>();
            if (_raycaster != null && _raycaster.Raycast(t.position, _hits, TrackableType.PlaneWithinPolygon))
            {
                var hit = _hits[0];
                PlaceAt(hit.pose);
            }
#else
            // In editor or without AR, place 1m in front of the camera.
            var cam = Camera.main;
            if (cam)
            {
                var pose = new Pose(cam.transform.position + cam.transform.forward * 1.0f, Quaternion.LookRotation(cam.transform.forward, Vector3.up));
                PlaceAt(pose);
            }
#endif
        }

        private void PlaceAt(Pose pose)
        {
            tableRoot.position = pose.position;
            tableRoot.rotation = pose.rotation;

            // Create or resize a simple table quad for visualization
            var vis = tableRoot.GetComponentInChildren<MeshFilter>();
            if (vis == null)
            {
                var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.name = "TableSurface";
                quad.transform.SetParent(tableRoot);
                quad.transform.localRotation = Quaternion.Euler(90, 0, 0); // lie flat
                vis = quad.GetComponent<MeshFilter>();
            }
            tableRoot.localScale = new Vector3(tableWidth, 1f, tableHeight);
        }
    }
}

