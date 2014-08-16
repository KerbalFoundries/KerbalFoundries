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
    public class OceanTest : MonoBehaviour
    {
        void Start()
        {
            print("OceanTest Start");
            int partCount = 0;
            int repulsorCount = 0;
            //var modules = new List<RepulsorAlpha>();
            foreach (Part PA in FlightGlobals.ActiveVessel.Parts)
            {
                partCount++;
                foreach (RepulsorTest RA in PA.GetComponentsInChildren<RepulsorTest>())
                {
                    repulsorCount++;
                }
            }
            print(partCount);
            print(repulsorCount);
            if (repulsorCount > 0)
            {
                print("Found some repulsors");
                FlightGlobals.ActiveVessel.rootPart.AddModule("ModuleWaterSlider");
            }
            else
            {
                print("did not find any repulsors");
            }
            
        }

    }

    public class ModuleWaterSlider : PartModule
    {
        GameObject _collider = new GameObject("ModuleWaterSlider.Collider", typeof(BoxCollider), typeof(Rigidbody));
        float triggerDistance = 100f; // avoid moving every frame

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
            visible.renderer.enabled = false; // enable to see collider

            // Debug stuff
            /*           Action<Vector3, Color> createAxis = delegate(Vector3 axis, Color c)
                       {
                           var go = new GameObject();
                           go.transform.parent = _collider.transform;
                           go.transform.position = _collider.transform.position + axis * 25f;

                           var line = DebugVisualizer.Line(new List<Transform> { go.transform, _collider.transform }, 0.2f, 0.1f);
                           line.renderer.material = ResourceUtil.LocateMaterial("DebugTools.XrayShader.shader", "Particles/Additive");
                           line.renderer.material.color = c;
                       };
            

                       createAxis(Vector3.up, Color.red);
                       createAxis(Vector3.right, Color.blue);
                       createAxis(Vector3.forward, Color.green);
                       */
            UpdatePosition();
        } 



        void UpdatePosition()
        {
           //print("Moving plane");

            var oceanNormal = part.vessel.mainBody.GetSurfaceNVector(vessel.latitude, vessel.longitude);

            _collider.rigidbody.position = (vessel.ReferenceTransform.position - oceanNormal * (FlightGlobals.getAltitudeAtPos(vessel.ReferenceTransform.position) + 2.6f));
            _collider.rigidbody.rotation = Quaternion.LookRotation(oceanNormal) * Quaternion.AngleAxis(90f, Vector3.right);
        }


        void FixedUpdate()
        {
            if (Vector3.Distance(_collider.transform.position, vessel.ReferenceTransform.position) > triggerDistance)
                UpdatePosition();
        }
    }
}
