using System;
using System.Linq;
using UnityEngine;

namespace KerbalFoundries
{
    public class OverrideWheelCollider : PartModule
    {
		//Log prefix to more easily identify this mod's log entries.
		public const string logprefix = "[KF - OverrideWheelCollider]: ";
		
        [KSPField]
        public string colliderName = "wheelCollider";
        [KSPField]
        public string susTravName = "suspensionTraverse";
        [KSPField]
        public float mass = 0; //no default
        [KSPField]
        public float radius = 0; //no default
        [KSPField]
        public float suspensionDistance = 0; //no default
        [KSPField]
        public float spring = 0; //no default
        [KSPField]
        public float damper = 0; //no default
        [KSPField]
        public float targetPosition = 0; //no default
        
        [KSPField]
        public float forExtSlip = 1;
        [KSPField]
        public float forExtValue = 120;
        [KSPField]
        public float forAsySlip = 2;
        [KSPField]
        public float forAsyValue = 80;
        [KSPField]
        public float forStiffness = 1;

        [KSPField]
        public float sideExtSlip = 1;
        [KSPField]
        public float sideExtValue = 120;
        [KSPField]
        public float sideAsySlip = 2;
        [KSPField]
        public float sideAsyValue = 80;
        [KSPField]
        public float sideStiffness = 1;

        [KSPField]
        public bool moveCollider = false;
        [KSPField]
        public bool overrideFriction = true;
        [KSPField]
        public float moveColliderBy;
        [KSPField]
        public int susTravIndex = 1;

        WheelCollider _wheelCollider;

        public override void OnStart(PartModule.StartState state)
        {
            GameObject _susTrav = this.part.FindModelTransform(susTravName).gameObject;
            foreach (var wc in this.part.GetComponentsInChildren<WheelCollider>())
            {
                if (wc.name.Equals(colliderName, StringComparison.Ordinal))
                    _wheelCollider = wc;
                else
					Debug.LogError(string.Format("{0}Wheel collider not found", logprefix));
            }

            if(HighLogic.LoadedSceneIsFlight)
            {
                if (moveCollider)
                {
                    _wheelCollider.transform.Translate(0, moveColliderBy, 0, Space.Self);
                    // SharpDevelop suggested we use "var" instead of reiterating "Vector3" in this definition. - Gaalidas
                    var tempVector = new Vector3(0, 0, 0);
                    //Vector3 tempVector = new Vector3(0, 0, 0);
                    tempVector[susTravIndex] = moveColliderBy;
                    _susTrav.transform.Translate(tempVector, Space.Self);
                }
            }

            if (_wheelCollider != null)
            {
                if(mass != 0)
                    _wheelCollider.mass = mass;
                if(radius != 0)
                    _wheelCollider.radius = radius;
                if (suspensionDistance != 0)
                    _wheelCollider.suspensionDistance = suspensionDistance;
                if (spring != 0)
                {
                    JointSpring js = _wheelCollider.suspensionSpring;
                    js.spring = spring;
                    _wheelCollider.suspensionSpring = js;
                }
                if (damper != 0)
                {
                    JointSpring js = _wheelCollider.suspensionSpring;
                    js.damper = damper;
                    _wheelCollider.suspensionSpring = js;
                }
                if (targetPosition != 0)
                { 
                    JointSpring js = _wheelCollider.suspensionSpring;
                    js.targetPosition = targetPosition;
                    _wheelCollider.suspensionSpring = js;
                }
                if (overrideFriction)
                {
                    WheelFrictionCurve _forwardFric = _wheelCollider.forwardFriction;
                    _forwardFric.extremumSlip = forExtSlip;
                    _forwardFric.extremumValue = forExtValue;
                    _forwardFric.asymptoteSlip = forAsySlip;
                    _forwardFric.asymptoteValue = forAsyValue;
                    _forwardFric.stiffness = forStiffness;
                    _wheelCollider.forwardFriction = _forwardFric;

                    WheelFrictionCurve _sideFric = _wheelCollider.sidewaysFriction;
                    _sideFric.extremumSlip = forExtSlip;
                    _sideFric.extremumValue = forExtValue;
                    _sideFric.asymptoteSlip = forAsySlip;
                    _sideFric.asymptoteValue = forAsyValue;
                    _sideFric.stiffness = forStiffness;
                    _wheelCollider.sidewaysFriction = _sideFric;
                }
            }
            base.OnStart(state);
        }
    }
}
