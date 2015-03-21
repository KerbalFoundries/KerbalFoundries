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
        bool sentOnRails;
        //[KSPField(isPersistant = true)]
        public bool isHitched;
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
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Joint Damper"), UI_FloatRange(minValue = 0, maxValue = 1f, stepIncrement = 0.1f)]
        public float jointDamper = 1;
        [KSPField(isPersistant = true)]
        public string _flightID;
        [KSPField(isPersistant = true)]
        public bool savedHitchState;
        public bool hitchCooloff;

        GameObject _targetObject;
        [KSPField(isPersistant = true,guiActive = true, guiName = "Target Object Name")]
        public string _targetObjectName;
        Rigidbody _rb;
        Part _targetPart;
        Vessel _targetVessel;
        
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

		//Log prefix to more easily identify this mod's log entries.
		public const string logprefix = "[KF - KFCouplingHitch]: ";

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Distance"), UI_FloatRange(minValue = 0, maxValue = 5f, stepIncrement = 0.2f)]
        public float rayDistance = 1;

        private LineRenderer line = null;

        IEnumerator HitchCooloffTimer()  
        {
            //print(Time.time);
            yield return new WaitForSeconds(10);
			print(string.Format("{0}Hitch Active", logprefix));
            hitchCooloff = false;
        }

        public void VesselPack(Vessel vessel)
        {
            if (vessel == this.vessel)
            {
                sentOnRails = true;
				Debug.LogError(string.Format("{0}Hitch state: {1}", logprefix, isHitched));
                //savedHitchState = isHitched;
                //GameEvents.onVesselGoOffRails.Remove(VesselUnPack);
                //GameEvents.onVesselGoOnRails.Remove(VesselPack);
            }
        }

        private IEnumerator WaitAndAttach() //Part partToAttach, Vector3 position, Quaternion rotation, Part toPart = null
        {

			Debug.Log(string.Format("{0}Wait for FixedUpdate", logprefix));
            yield return new WaitForFixedUpdate();
			Debug.LogError(string.Format("{0}Saved hitch state{1}", logprefix, savedHitchState));
            
            
            if (savedHitchState)
            {
				Debug.LogWarning(string.Format("{0}Was previously hitched at last save", logprefix));
				print(string.Format("{0}Taget flightID: {1}", logprefix, _flightID));
				print(string.Format("{0}Compare ID with flight ID: {1}", logprefix, uint.Parse(_flightID)));
                foreach (Part pa in FindObjectsOfType(typeof(Part)) as Part[])
                {
					print(string.Format("{0}Found part with flight ID {1}", logprefix, pa.flightID));
                    

                    if (pa.flightID.Equals(uint.Parse(_flightID)))
                    {
                        //UnHitch(); //just in case, by some miracle, we're hitched already
                        _targetPart = pa;

						Debug.LogWarning(string.Format("{0}Found part from persistence", logprefix));
                        _targetObject = pa.transform.Search(_targetObjectName).gameObject;
                        _rb = pa.rigidbody;

						Debug.LogWarning(string.Format("{0}Found hitchObject from persistence", logprefix));
                        _targetObject.transform.position = _hitchObject.transform.position;
						Debug.LogWarning(string.Format("{0}Put objects in correct position", logprefix));
                        //RayCast(0.3f);

                        Hitch();
						Debug.LogError(string.Format("{0}Hitched", logprefix));

                    }
                }
            }
            else //string is nullorempty
				Debug.LogError(string.Format("{0}Not previously hitched", logprefix));
            isReady = true;
        }

                

        

        public void VesselUnPack(Vessel vessel)
        {
            Debug.LogWarning(string.Format("{0}FlightChecker started", logprefix));
            if(vessel == this.vessel)
                StartCoroutine("WaitAndAttach");
            //were we previously hitched, is this the vessel this part is attached to firing the event, were we sent on rails without setting up fresh and needing to hitch again.
            if (savedHitchState && vessel == this.vessel && !sentOnRails) 
            {
                
            }
            
        }

        //[KSPEvent(guiActive = true, guiName = "Hitch", active = true)]
        void Hitch()
        {
            if (_targetObject != null)
            {
                isHitched = true;
                savedHitchState = true;
				Debug.LogWarning(string.Format("{0}Start of method...", logprefix));
                _couplingObject = _targetObject;
                _targetObjectName = _targetObject.name.ToString();

                _targetPart = _targetObject.GetComponentInParent<Part>() as Part;
                _flightID = _targetPart.flightID.ToString();
				print(string.Format("{0}Target flight ID {1}", logprefix, _flightID));
                //print(_targetPart.launchID);
                //print(_targetPart.name);


                _targetVessel = _targetPart.vessel;
				print(string.Format("{0}Vessel ID {1}", logprefix, _targetVessel.GetInstanceID()));

				Debug.LogWarning(string.Format("{0}Set up vessel and part stuff", logprefix));

                //_hitchObject.transform.rotation = _couplingObject.transform.rotation;
                _rbLink = _Link.gameObject.AddComponent<Rigidbody>();
                _rbLink.mass = 0.01f;
                _rbLink.useGravity = false;
                _HitchJoint = this.part.gameObject.AddComponent<ConfigurableJoint>();
                _LinkJoint = _Link.gameObject.AddComponent<ConfigurableJoint>();

				#if Debug
				Debug.LogWarning(string.Format("{0}Created joint...", logprefix));
				#endif
                _LinkJoint.xMotion = ConfigurableJointMotion.Locked;
                _LinkJoint.yMotion = ConfigurableJointMotion.Locked;
                _LinkJoint.zMotion = ConfigurableJointMotion.Locked;

                _HitchJoint.xMotion = ConfigurableJointMotion.Locked;
                _HitchJoint.yMotion = ConfigurableJointMotion.Locked;
                _HitchJoint.zMotion = ConfigurableJointMotion.Locked;

				#if Debug
				Debug.LogWarning(string.Format("{0}Configured linear...", logprefix));
				#endif //set up X limits
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

				#if Debug
				Debug.LogWarning(string.Format("{0}Configured linear...", logprefix));
				#endif

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


                SetJointDamper();

				#if Debug
				Debug.LogWarning(string.Format("{0}Configured joint...", logprefix));
				#endif

                _HitchJoint.anchor = new Vector3(0, 0.4f, 0); //this seems to make a springy joint

                // Set correct axis
                //_HitchJoint.axis = new Vector3(1, 0, 0);
                //_HitchJoint.secondaryAxis = new Vector3(0, 0, 1);
				#if Debug
				Debug.LogWarning(string.Format("{0}Configured axis...", logprefix));
                #endif
                _HitchJoint.connectedBody = _rbLink;

                _Link.transform.rotation = _couplingObject.transform.rotation;
				#if Debug
				Debug.LogWarning(string.Format("{0}Connected joint...", logprefix));
				#endif

				print(string.Format("{0}Target object is {1}", logprefix, _couplingObject.name));
                _couplingObject.transform.position = _hitchObject.transform.position;
                _LinkJoint.connectedBody = _rb;
            }
            else
				Debug.LogWarning(string.Format("{0}No target", logprefix));
        }

        [KSPEvent(guiActive = true, guiName = "Update Damper", active = true, guiActiveUnfocused = true, unfocusedRange = 40f)]
        void SetJointDamper()
        {
            if (_HitchJoint != null)
            {
                _HitchJoint.rotationDriveMode = RotationDriveMode.XYAndZ;
                JointDrive X = _HitchJoint.angularXDrive;
                X.mode = JointDriveMode.Position;
                X.positionDamper = jointDamper;
                _HitchJoint.angularXDrive = X;
                JointDrive YZ = _HitchJoint.angularYZDrive;
                YZ.mode = JointDriveMode.Position;
                YZ.positionDamper = jointDamper;
                _HitchJoint.angularYZDrive = YZ;
            }
            else
            {
				Debug.LogError(string.Format("{0}No joint to update!", logprefix));
            }
        }

        [KSPEvent(guiActive = true, guiName = "Un-Hitch", active = true, guiActiveUnfocused = true, unfocusedRange = 40f)]
        void UnHitch()
        {
            if (_LinkJoint != null)
            {
				Debug.LogWarning(string.Format("{0}Unhitching...", logprefix));
                //_joint.connectedBody = this.part.rigidbody;
                GameObject.Destroy(_LinkJoint);
                GameObject.Destroy(_rbLink);
                GameObject.Destroy(_HitchJoint);
                _Link.transform.localEulerAngles = _LinkRotation;
                isHitched = false;
                savedHitchState = false;
                hitchCooloff = true;
                _couplingObject = null;
                _flightID = string.Empty;
                StartCoroutine("HitchCooloffTimer");
            }
            else
				Debug.LogWarning(string.Format("{0}Not hitched!!!!", logprefix));
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

			#if Debug
            if (isDebug)
            {
				print(string.Format("{0}normalVector: {1}", logprefix, normalvectorX));
				print(string.Format("{0}normalVector: {1}", logprefix, normalvectorY));
				print(string.Format("{0}normalVector: {1}", logprefix, normalvectorZ));
				Debug.LogWarning(string.Format("{0}Rotate: {1}", logprefix, hitchRotation));
            }
			#endif
            Vector3 couplingRotation = new Vector3(angleX, angleY, angleZ);

            return couplingRotation;

        }

        public override void OnActive()
        {
            base.OnActive();
			Debug.LogError(string.Format("{0}Adding Hooks", logprefix));
            
        }

        public override void OnInactive()
        {
            base.OnInactive();
			Debug.LogError(string.Format("{0}Removing Hooks", logprefix));
            //GameEvents.onVesselGoOffRails.Remove(VesselUnPack);
            //GameEvents.onVesselGoOnRails.Remove(VesselPack);
        }

        void OnDestroy()
        {
			Debug.LogError(string.Format("{0}OnDestroy", logprefix));
            GameEvents.onVesselGoOffRails.Remove(VesselUnPack);
            GameEvents.onVesselGoOnRails.Remove(VesselPack);
			Debug.LogError(string.Format("{0}Hitch State destroy{1}", logprefix, isHitched));
            //savedHitchState = isHitched;

			//Debug.LogError(string.Format("{0}Vessel Pack", logprefix));
            if (isHitched)
            {

            }
        }


        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            GameEvents.onVesselGoOffRails.Add(VesselUnPack);
            GameEvents.onVesselGoOnRails.Add(VesselPack);
            
            _hitchObject = transform.Search(hitchObjectName).gameObject;
            _Link = transform.Search(hitchLinkName).gameObject;
            _LinkRotation = _Link.transform.localEulerAngles;

            if (HighLogic.LoadedSceneIsFlight)
            {
                
                // First of all, create a GameObject to which LineRenderer will be attached.
                //GameObject obj = _hitchObject.gameObject;

                // Then create renderer itself...
                DebugLine(_hitchObject.gameObject, _hitchObject.transform.forward, Color.black);
                //StartCoroutine("FlightChecker");
                
                

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
            bool brakesOn = FlightGlobals.ActiveVessel.ActionGroups[KSPActionGroup.Brakes];

            if(isHitched)
            {

                _targetVessel.ActionGroups.SetGroup(KSPActionGroup.Brakes, brakesOn);

            }

            if (_targetObject != null && !isHitched)
            {

                if (_targetObject.name.Equals("EyeTarget", StringComparison.Ordinal) || _targetObject.name.Equals("EyePoint", StringComparison.Ordinal))
                {
                    Vector3 forceVector = -(_targetObject.transform.position - _hitchObject.transform.position).normalized;

                    Vector3 forcePlane = forceVector - (_hitchObject.transform.forward) * Vector3.Dot(forceVector, _hitchObject.transform.forward);
                    Vector3 force = forcePlane * forceMultiplier * Mathf.Clamp((1 / (_targetObject.transform.position - _hitchObject.transform.position).magnitude), -maxForce, maxForce); //(1 / (_targetObject.transform.position - _hitchObject.transform.position).magnitude) *
                    _rb.rigidbody.AddForceAtPosition(force, _targetObject.transform.position);
                    this.part.rigidbody.AddForceAtPosition(-force, _hitchObject.transform.position);
                }


                if (_targetObject.name.Equals("EyePoint", StringComparison.Ordinal))
                {
                    
                    if (Vector3.Distance(_targetObject.transform.position, _hitchObject.transform.position) < 0.1f)
                    {
                        Vector3 jointLimitCheck = JointRotation(_hitchObject.transform, _targetObject.transform, false);
                        bool rotationCorrect = false;
                        if (jointLimitCheck[0] < xLimitHigh && jointLimitCheck[1] < yLimit && jointLimitCheck[2] < zLimit)
                            rotationCorrect = true;
                        else
						Debug.Log(string.Format("{0}Joint outside rotation limit", logprefix));

                        if (rotationCorrect)
                        {
						Debug.Log(string.Format("{0}Rotation within limits, hitching", logprefix));
                            Hitch();
                        }
                    }
                    else
                        Debug.Log(string.Format("{0}Not close enough to couple", logprefix));

                    //Debug.Log(string.Format("{0}Found HitchPoint, hitching", logprefix));
                }
            }


			//print(_joint.)
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
                    _targetPart = Part.FromGO(hit.rigidbody.gameObject);
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
