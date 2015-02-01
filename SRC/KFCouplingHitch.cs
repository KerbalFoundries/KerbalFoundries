using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalFoundries
{
    public class KFCouplingHitch : PartModule
    {
        bool isReady;
        bool isHitched;
        [KSPField]
        public string hitchObjectName;
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Coupling Force"), UI_FloatRange(minValue = 0, maxValue = 10f, stepIncrement = 1f)]
        float forceMultiplier = 1;
        [KSPField]
        public string hitchLinkName;
        [KSPField]
        public float maxForce = 7.5f;
        [KSPField]
        public float xLimitHigh = 20;
        [KSPField]
        public float xLimitLow = 20;
        [KSPField]
        public float yLimit = 20;
        [KSPField]
        public float zLimit = 20; 
        [KSPField(guiActive = true, guiUnits = "deg", guiName = "Hitch Angle", guiFormat = "F0")]
        public Vector3 hitchRotation;
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = false, guiName = "Debug"), UI_Toggle(disabledText = "Enabled", enabledText = "Disabled")]
        public bool isDebug = false;
        public bool hitchCooloff;

        GameObject _targetObject;
        Rigidbody _rb;
        Part _part;
        GameObject _hitchObject;
        GameObject _couplingObject;
        GameObject _Link;
        ConfigurableJoint _LinkJoint;
        ConfigurableJoint _HitchJoint;
        Rigidbody _rbLink;
        Vector3 _LinkRotation;
        [KSPField]
        public int layerMask = 0;
        [KSPField(guiName = "Ray", guiActive = true)]
        public string ravInfo;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Distance"), UI_FloatRange(minValue = 0, maxValue = 5f, stepIncrement = 0.2f)]
        public float rayDistance = 1;

        private LineRenderer line = null;

        IEnumerator HitchCooloffTimer() //coroutine for tracked wheels (all rotate the same speed in the part) 
        {
            //_wheel.transform.Rotate(_wheelRotation, _KFModuleWheel.degreesPerTick * directionCorrector * rotationCorrection); //rotate wheel
            print(Time.time);
            yield return new WaitForSeconds(10);
            print(Time.time);
            hitchCooloff = false;
        }

        [KSPEvent(guiActive = true, guiName = "Hitch", active = true)]
        void Hitch()
        {
            if (_targetObject != null)
            {
                isHitched = true;
                Debug.LogWarning("Start of method...");
                _couplingObject = _targetObject;
                //_hitchObject.transform.rotation = _couplingObject.transform.rotation;
                _rbLink = _Link.gameObject.AddComponent<Rigidbody>();
                _rbLink.mass = 0.01f;
                _rbLink.useGravity = false;
                _HitchJoint = this.part.gameObject.AddComponent<ConfigurableJoint>();
                _LinkJoint = _Link.gameObject.AddComponent<ConfigurableJoint>();


                Debug.LogWarning("Created joint...");
                _LinkJoint.xMotion = ConfigurableJointMotion.Locked;
                _LinkJoint.yMotion = ConfigurableJointMotion.Locked;
                _LinkJoint.zMotion = ConfigurableJointMotion.Locked;

                _HitchJoint.xMotion = ConfigurableJointMotion.Locked;
                _HitchJoint.yMotion = ConfigurableJointMotion.Locked;
                _HitchJoint.zMotion = ConfigurableJointMotion.Locked;


                Debug.LogWarning("Configured linear...");
                //set up X limits
                SoftJointLimit sjl;
                sjl = _HitchJoint.highAngularXLimit;
                sjl.limit = xLimitHigh;
                _HitchJoint.highAngularXLimit = sjl;
                sjl = _HitchJoint.lowAngularXLimit;
                sjl.limit = -xLimitLow;
                _HitchJoint.lowAngularXLimit = sjl;


                //set up Y limits
                sjl = _HitchJoint.angularYLimit;
                sjl.limit = yLimit;
                _HitchJoint.angularYLimit = sjl;
                //set up Z limitssssssa
                sjl = _HitchJoint.angularZLimit;
                sjl.limit = zLimit;
                _HitchJoint.angularZLimit = sjl;



                Debug.LogWarning("Configured linear...");
                _HitchJoint.angularXMotion = ConfigurableJointMotion.Limited;
                _HitchJoint.angularYMotion = ConfigurableJointMotion.Limited;
                _HitchJoint.angularZMotion = ConfigurableJointMotion.Limited;
                _HitchJoint.projectionMode = JointProjectionMode.PositionOnly;
                _HitchJoint.projectionDistance = 0.05f;

                _LinkJoint.angularXMotion = ConfigurableJointMotion.Locked;
                _LinkJoint.angularYMotion = ConfigurableJointMotion.Locked;
                _LinkJoint.angularZMotion = ConfigurableJointMotion.Locked;
                _LinkJoint.projectionMode = JointProjectionMode.PositionOnly;
                _LinkJoint.projectionDistance = 0.05f;

                Debug.LogWarning("Configured joint...");

                _HitchJoint.anchor = new Vector3(0, 0.4f, 0); //this seems to make a springy joint

                // Set correct axis
                //_HitchJoint.axis = new Vector3(1, 0, 0);
                //_HitchJoint.secondaryAxis = new Vector3(0, 0, 1);

                Debug.LogWarning("Configured axis...");
                _HitchJoint.connectedBody = _rbLink;

                _Link.transform.rotation = _couplingObject.transform.rotation;

                Debug.LogWarning("Connected joint...");


                print("Target object is " + _couplingObject.name);
                _couplingObject.transform.position = _hitchObject.transform.position;
                _LinkJoint.connectedBody = _rb;
            }
            else
                Debug.LogWarning("No target");
        }

        [KSPEvent(guiActive = true, guiName = "Un-Hitch", active = true)]
        void UnHitch()
        {
            if (_LinkJoint != null)
            {
                Debug.LogWarning("Unhitching...");
                //_joint.connectedBody = this.part.rigidbody;
                GameObject.Destroy(_LinkJoint);
                GameObject.Destroy(_rbLink);
                GameObject.Destroy(_HitchJoint);
                _Link.transform.localEulerAngles = _LinkRotation;
                isHitched = false;
                hitchCooloff = true;
                _couplingObject = null;
                StartCoroutine("HitchCooloffTimer");
            }
            else
                Debug.LogWarning("Not hitched!!!!");
        }


        //[KSPEvent(guiActive = true, guiName = "Show rotation", active = true)]
        public Vector3 JointRotation(Transform hitch, Transform coupling, bool signed)
        {
            //this projects vectors onto chosen 2D planes. planes are defined by their normals, in this case hitchObject.transform.forward.
            Vector3 hitchProjectX = hitch.transform.right - (hitch.transform.forward) * Vector3.Dot(hitch.transform.right, hitch.transform.forward);
            Vector3 attachProjectX = coupling.transform.right - (hitch.transform.forward) * Vector3.Dot(coupling.transform.right, hitch.transform.forward);

            Vector3 hitchProjectY = hitch.transform.up - (hitch.transform.right) * Vector3.Dot(hitch.transform.up, hitch.transform.right);
            Vector3 attachProjectY = coupling.transform.up - (hitch.transform.right) * Vector3.Dot(coupling.transform.up, hitch.transform.right);

            Vector3 hitchProjectZ = hitch.transform.forward - (hitch.transform.up) * Vector3.Dot(hitch.transform.forward, hitch.transform.up);
            Vector3 attachProjectZ = coupling.transform.forward - (hitch.transform.up) * Vector3.Dot(coupling.transform.forward, hitch.transform.up);

            float angleX = Mathf.Acos(Vector3.Dot(hitchProjectX, attachProjectX) / Mathf.Sqrt(Mathf.Pow(hitchProjectX.magnitude, 2) * Mathf.Pow(attachProjectX.magnitude, 2))) * Mathf.Rad2Deg;
            float angleY = Mathf.Acos(Vector3.Dot(hitchProjectY, attachProjectY) / Mathf.Sqrt(Mathf.Pow(hitchProjectY.magnitude, 2) * Mathf.Pow(attachProjectY.magnitude, 2))) * Mathf.Rad2Deg;
            float angleZ = Mathf.Acos(Vector3.Dot(hitchProjectZ, attachProjectZ) / Mathf.Sqrt(Mathf.Pow(hitchProjectZ.magnitude, 2) * Mathf.Pow(attachProjectZ.magnitude, 2))) * Mathf.Rad2Deg;

            if (signed)
            {
                Vector3 normalvectorX = Vector3.Cross(hitchProjectX, attachProjectX);
                if (normalvectorX[2] < 0.0f)
                    angleX *= -1;
                Vector3 normalvectorY = Vector3.Cross(hitchProjectY, attachProjectY);
                if (normalvectorY[1] < 0.0f)
                    angleY *= -1;
                Vector3 normalvectorZ = Vector3.Cross(hitchProjectZ, attachProjectZ);
                if (normalvectorZ[0] < 0.0f)
                    angleZ *= -1;
            }

            /*
            if (isDebug)
            {
                print("normalVector" + normalvectorX);
                print("normalVector" + normalvectorY);
                print("normalVector" + normalvectorZ);
                
                Debug.LogWarning("Rotate " + hitchRotation);
            }
             * */
            Vector3 couplingRotation = new Vector3(angleX, angleY, angleZ);

            return couplingRotation;

        }


        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            _hitchObject = transform.Search(hitchObjectName).gameObject;
            _Link = transform.Search(hitchLinkName).gameObject;
            _LinkRotation = _Link.transform.localEulerAngles;
            if (HighLogic.LoadedSceneIsFlight)
            {
                isReady = true;
                // First of all, create a GameObject to which LineRenderer will be attached.
                //GameObject obj = _hitchObject.gameObject;

                // Then create renderer itself...
                DebugLine(_hitchObject.gameObject, _hitchObject.transform.forward, Color.black);

            }
        }

        void DebugLine(GameObject obj, Vector3 dir, Color c)
        {
            line = obj.AddComponent<LineRenderer>();
            //line.transform.parent = transform; // ...child to our part...
            line.useWorldSpace = false; // ...and moving along with it (rather 
            // than staying in fixed world coordinates)
            //line.transform.localPosition = Vector3.zero;
            //line.transform.localEulerAngles = Vector3.zero;

            // Make it render a red to yellow triangle, 1 meter wide and 2 meters long
            line.material = new Material(Shader.Find("Particles/Additive"));
            line.SetColors(c, Color.white);
            line.SetWidth(0.1f, 0.1f);
            line.SetVertexCount(2);
            line.SetPosition(0, Vector3.zero);
            line.SetPosition(1, Vector3.forward * 2);
        }

        public void FixedUpdate()
        {
            if (!isReady || hitchCooloff)
                return;
            RayCast(rayDistance);
            if (_couplingObject != null)
            {
                //ShowJointRotation();
            }

            if (_targetObject != null && !isHitched)
            {

                if (_targetObject.name.Equals("EyeTarget", StringComparison.Ordinal) || _targetObject.name.Equals("EyePoint", StringComparison.Ordinal) && !isHitched)
                {
                    Vector3 forceVector = -(_targetObject.transform.position - _hitchObject.transform.position).normalized;

                    Vector3 forcePlane = forceVector - (_hitchObject.transform.forward) * Vector3.Dot(forceVector, _hitchObject.transform.forward);
                    Vector3 force = forcePlane * forceMultiplier * Mathf.Clamp((1 / (_targetObject.transform.position - _hitchObject.transform.position).magnitude), -maxForce, maxForce); //(1 / (_targetObject.transform.position - _hitchObject.transform.position).magnitude) *
                    //Debug.Log("force is " + force);
                    //Debug.Log((1 / (_targetObject.transform.position - _hitchObject.transform.position).magnitude));
                    _rb.rigidbody.AddForceAtPosition(force, _targetObject.transform.position);
                }
                //else
                //Debug.Log("Not EyeTarget");

                if (_targetObject.name.Equals("EyePoint", StringComparison.Ordinal) && !isHitched)
                {
                    //RayCast(0.2f);
                    if (Vector3.Distance(_targetObject.transform.position, _hitchObject.transform.position) < 0.1f)
                    {
                        Vector3 jointLimitCheck = JointRotation(_hitchObject.transform, _targetObject.transform, false);
                        bool rotationCorrect = false;
                        if (jointLimitCheck[0] < xLimitHigh && jointLimitCheck[1] < yLimit && jointLimitCheck[2] < zLimit)
                            rotationCorrect = true;
                        else
                            Debug.Log("Joint outside rotation limit");

                        if (rotationCorrect)
                        {
                            Hitch();

                            Debug.Log("Rotation within limits, hitching");
                        }
                    }
                    else
                        Debug.Log("Not close enough");

                    //Debug.Log("Found HitchPoint, hitching");
                }
            }


            //print(_joint.
        }

        //[KSPEvent(guiActive = true, guiName = "Fire Ray", active = true)]
        void RayCast(float rayLength)
        {
            Ray ray = new Ray(_hitchObject.transform.position, _hitchObject.transform.forward);
            RaycastHit hit;
            int tempLayerMask = ~layerMask;
            //Debug.DrawRay(_hitchObject.transform.position, _hitchObject.transform.up);
            line.transform.rotation = _hitchObject.transform.rotation;
            line.transform.position = _hitchObject.transform.position;
            if (Physics.Raycast(ray, out hit, rayLength, tempLayerMask))
            {
                //targetObject = hit.collider.gameObject;
                ravInfo = hit.collider.gameObject.name.ToString();
                try
                {
                    _part = Part.FromGO(hit.rigidbody.gameObject);
                    _rb = (hit.rigidbody);
                    //if(p.vessel != this.vessel)
                    //{
                    _targetObject = hit.collider.gameObject;
                    //print("Hit " + hit.collider.gameObject.name);
                    //}

                }
                catch (NullReferenceException) { }

            }
            else
            {
                ravInfo = "Nothing";
                _targetObject = null;
            }
        }
    }
}
