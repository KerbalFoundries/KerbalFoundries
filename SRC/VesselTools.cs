/*
 * KSP [0.23.5] Anti-Grav Repulsor plugin by Lo-Fi
 * HUGE thanks to xEvilReeperx for this water code, along with gerneral coding help along the way!
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalFoundries
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class RepulsorSkim : MonoBehaviour
    {
        void Start()
        {
            print("RepulsorSkim Start");
            int partCount = 0;
            int repulsorCount = 0;
            foreach (Part PA in FlightGlobals.ActiveVessel.Parts)
            {
                partCount++;
                foreach (RepulsorTest RA in PA.GetComponentsInChildren<RepulsorTest>())
                {
                    repulsorCount++;
                }
                foreach (AlphaRepulsor RA in PA.GetComponentsInChildren<AlphaRepulsor>())
                {
                    repulsorCount++;
                }

                foreach (RepulsorWheel RA in PA.GetComponentsInChildren<RepulsorWheel>())
                {
                    repulsorCount++;
                }
            }

            if (repulsorCount > 0)
            {
                FlightGlobals.ActiveVessel.rootPart.AddModule("ModuleWaterSlider");
            }

        }

    }

    public class ModuleWaterSlider : PartModule
    {
        GameObject _collider = new GameObject("ModuleWaterSlider.Collider", typeof(BoxCollider), typeof(Rigidbody));
        float triggerDistance = 100f; // avoid moving every frame

        public float colliderHeight = 10f;

        float currentColliderHeight;

        public bool isColliderEnabled = true;
         
        void Start()
        {
            print("WaterSlider start");

            var box = _collider.collider as BoxCollider;
            box.size = new Vector3(400f, 5f, 400f); // probably should encapsulate other colliders in real code

            var rb = _collider.rigidbody;
            rb.isKinematic = true;

            _collider.SetActive(true);

            var visible = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visible.transform.parent = _collider.transform;
            visible.transform.localScale = box.size;
            visible.renderer.enabled = true; // enable to see collider
            currentColliderHeight = 10;
            UpdatePosition();
        }

        void UpdatePosition()
        {
                var oceanNormal = part.vessel.mainBody.GetSurfaceNVector(vessel.latitude, vessel.longitude);
                
                print(currentColliderHeight);
                _collider.rigidbody.position = (vessel.ReferenceTransform.position - oceanNormal * (FlightGlobals.getAltitudeAtPos(vessel.ReferenceTransform.position) + currentColliderHeight));
                _collider.rigidbody.rotation = Quaternion.LookRotation(oceanNormal) * Quaternion.AngleAxis(90f, Vector3.right);
        }

        void FixedUpdate()
        {
            currentColliderHeight = Mathf.Lerp(colliderHeight, currentColliderHeight, 25 * Time.deltaTime);
            if (Vector3.Distance(_collider.transform.position, vessel.ReferenceTransform.position) > triggerDistance || currentColliderHeight != Math.Round(colliderHeight,1))
                UpdatePosition();
        }
    }
}
