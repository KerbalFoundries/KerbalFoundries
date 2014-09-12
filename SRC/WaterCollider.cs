using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalFoundries
{
    public class ModuleWaterCollider : PartModule
    {
        GameObject _collider = new GameObject("ModuleWaterCollider.Collider", typeof(BoxCollider), typeof(Rigidbody));
        float triggerDistance = 100f; // avoid moving every frame
        

        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Floatation depth"), UI_FloatRange(minValue = 0, maxValue = 8.0f, stepIncrement = 0.25f)]
        float colliderHeight = 3.5f;

        void Start()
        {
            print("Water hovering enabled");

           
            foreach (Part PA in FlightGlobals.ActiveVessel.Parts)
            {
                if (PA.GetComponentInChildren<RepulsorTest>() != null || PA.GetComponentInChildren<AlphaRepulsor>() != null)
                {
                    var box = _collider.collider as BoxCollider;
                    box.size = new Vector3(400f, 5f, 400f); // probably should encapsulate other colliders in real code

                    var rb = _collider.rigidbody;
                    rb.isKinematic = true;

                    _collider.SetActive(true);

                    var visible = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    visible.transform.parent = _collider.transform;
                    visible.transform.localScale = box.size;
                    visible.renderer.enabled = false; // enable to see collider

                    UpdatePosition(); 
                    this.part.force_activate(); 
                }
            }   

        }

        void UpdatePosition()
        {
            //print("Moving plane");

            var oceanNormal = part.vessel.mainBody.GetSurfaceNVector(vessel.latitude, vessel.longitude);

            _collider.rigidbody.position = (vessel.ReferenceTransform.position - oceanNormal * (FlightGlobals.getAltitudeAtPos(vessel.ReferenceTransform.position) + colliderHeight));
            _collider.rigidbody.rotation = Quaternion.LookRotation(oceanNormal) * Quaternion.AngleAxis(90f, Vector3.right);
        }

        public override void OnFixedUpdate()
        {
            if (Vector3.Distance(_collider.transform.position, vessel.ReferenceTransform.position) > triggerDistance)
                UpdatePosition();
        }
    }
}
