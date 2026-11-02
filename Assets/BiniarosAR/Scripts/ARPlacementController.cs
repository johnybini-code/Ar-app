using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace BiniarosAR
{
    /// Places the pergola at real-world scale on detected horizontal planes.
    /// One finger tap: place/move. Two-finger twist: rotate.
    public class ARPlacementController : MonoBehaviour
    {
        public ARRaycastManager raycastManager;
        public ARPlaneManager planeManager;
        public Camera arCamera;
        public Transform pergolaRoot;
        public bool hidePlanesAfterPlacement = true;

        readonly List<ARRaycastHit> hits = new();
        bool placed;
        float previousTwist;

        void Awake()
        {
            if (pergolaRoot != null) pergolaRoot.gameObject.SetActive(false);
        }

        void Update()
        {
            HandlePlacement();
            HandleRotation();
        }

        void HandlePlacement()
        {
            if (Input.touchCount != 1) return;
            Touch touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Began) return;

            if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;
                float yaw = arCamera != null ? arCamera.transform.eulerAngles.y : 0f;

                pergolaRoot.SetPositionAndRotation(hitPose.position, Quaternion.Euler(0f, yaw, 0f));
                pergolaRoot.localScale = Vector3.one; // 1 Unity unit = 1 meter
                pergolaRoot.gameObject.SetActive(true);
                placed = true;

                if (hidePlanesAfterPlacement && planeManager != null)
                    SetPlaneVisuals(false);
            }
        }

        void HandleRotation()
        {
            if (!placed || Input.touchCount != 2) return;
            Touch a = Input.GetTouch(0);
            Touch b = Input.GetTouch(1);
            Vector2 d = b.position - a.position;
            float angle = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;

            if (a.phase == TouchPhase.Began || b.phase == TouchPhase.Began)
            {
                previousTwist = angle;
                return;
            }

            float delta = Mathf.DeltaAngle(previousTwist, angle);
            pergolaRoot.Rotate(Vector3.up, -delta, Space.World);
            previousTwist = angle;
        }

        public void ResetPlacement()
        {
            placed = false;
            if (pergolaRoot != null) pergolaRoot.gameObject.SetActive(false);
            SetPlaneVisuals(true);
        }

        public void Rotate90()
        {
            if (pergolaRoot != null) pergolaRoot.Rotate(Vector3.up, 90f, Space.World);
        }

        void SetPlaneVisuals(bool enabled)
        {
            if (planeManager == null) return;
            foreach (var plane in planeManager.trackables)
            {
                var renderer = plane.GetComponent<MeshRenderer>();
                if (renderer != null) renderer.enabled = enabled;
            }
        }
    }
}
