using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalFoundries
{
    [KSPModule("TrackWheel")]
    public class TrackWheel : PartModule
    {
        //config fields
        [KSPField]
        public string wheelName;
        [KSPField]
        public string colliderName;
        [KSPField]
        public string sustravName;
        [KSPField]
        public string steeringName;
        [KSPField]
        public string susNeutralName;
        [KSPField]
        public bool useDirectionCorrector = false; //make sure it's set to false if not specified in the config
        [KSPField]
        public bool isSprocket = false;
        [KSPField]
        public bool isIdler = false;
        [KSPField]
        public float smoothSpeed = 40;
        [KSPField(isPersistant = true)]
        public float suspensionDistance;
        [KSPField]
        public float rotationCorrection = 1;
            //wheel rotation axis
        [KSPField]
        public float wheelRotationX = 1;
        [KSPField]
        public float wheelRotationY = 0;
        [KSPField]
        public float wheelRotationZ = 0;
            //suspension traverse axis
        [KSPField]
        public string susTravAxis = "Y";
        [KSPField]
        public string steeringAxis = "Y";
        [KSPField(isPersistant = true)]
        public float lastFrameTraverse;

        //object types
        WheelCollider _wheelCollider;
        Transform _susTrav;
        Transform _wheel;
        Transform _trackSteering;
        ModuleTrack _track;

        //gloabl variables
        Vector3 initialPosition;
        Vector3 initialSteeringAngles;
        Transform susStart;
        Vector3 _wheelRotation;
        int susTravIndex = 1;
        int steeringIndex = 1;
        public int directionCorrector = 1;
        

        //OnStart
        public override void OnStart(PartModule.StartState state)
        {
            if (suspensionDistance == 0 & !isSprocket)
            {
                foreach (WheelCollider wc in this.part.GetComponentsInChildren<WheelCollider>())
                {
                    if (wc.name.StartsWith(colliderName, StringComparison.Ordinal))
                    {
                        _wheelCollider = wc;
                        suspensionDistance = wc.suspensionDistance;
                        print("suspensionDistance is");
                        print(suspensionDistance);
                    }
                }
            }

            if (HighLogic.LoadedSceneIsEditor)
            {

            }
            
            if (HighLogic.LoadedSceneIsFlight)
            {
                //find named onjects in part
                foreach (WheelCollider wc in this.part.GetComponentsInChildren<WheelCollider>())
                {
                    if (wc.name.StartsWith(colliderName, StringComparison.Ordinal))
                    {
                        _wheelCollider = wc;
                    }
                }
                foreach (Transform tr in this.part.GetComponentsInChildren<Transform>())
                {
                    if (tr.name.StartsWith(wheelName, StringComparison.Ordinal))
                    {
                        _wheel = tr;
                    }
                    if (tr.name.StartsWith(steeringName, StringComparison.Ordinal))
                    {
                        _trackSteering = tr;
                    }
                    if (tr.name.StartsWith(sustravName, StringComparison.Ordinal))
                    {
                        _susTrav = tr;
                    }
                    if (tr.name.StartsWith(susNeutralName, StringComparison.Ordinal))
                    {
                        susStart = tr;
                    }
                }
                //end find named objects

                _track = this.part.GetComponentInChildren<ModuleTrack>();

                susTravIndex = Extensions.SetAxisIndex(susTravAxis);
                steeringIndex = Extensions.SetAxisIndex(steeringAxis);

                if (_track.hasSteering)
                {
                    initialSteeringAngles = _trackSteering.transform.localEulerAngles;
                    //print(initialSteeringAngles);
                }

                if (useDirectionCorrector)
                    directionCorrector = _track.directionCorrector;
                else directionCorrector = 1;

                _wheelRotation = new Vector3(wheelRotationX, wheelRotationY, wheelRotationZ);

                initialPosition = _susTrav.transform.localPosition;
                if (lastFrameTraverse == 0) //check to see if we have a value in persistance
                {
                    Debug.LogError("Last frame = 0. Setting");
                    lastFrameTraverse = _wheelCollider.suspensionDistance;
                    Debug.LogError(lastFrameTraverse);
                }
                Debug.LogError("Last frame =");
                Debug.LogError(lastFrameTraverse);
                moveSuspension(initialPosition, susTravIndex, lastFrameTraverse, _susTrav); //to get the initial stuff correct
            }
            base.OnStart(state);
            this.part.force_activate(); 
        }//end OnStart
        //OnUpdate

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            _wheel.transform.Rotate(_wheelRotation, _track.degreesPerTick * directionCorrector * rotationCorrection); //rotate wheel
            _wheelCollider.suspensionDistance = suspensionDistance * _track.appliedRideHeight;
            //suspension movement
            WheelHit hit;
            float frameTraverse = 0;
            bool grounded = _wheelCollider.GetGroundHit(out hit); //set up to pass out wheelhit coordinates
            if (grounded && !isSprocket) //is it on the ground
            {
                frameTraverse = -_wheelCollider.transform.InverseTransformPoint(hit.point).y + _track.raycastError - _wheelCollider.radius;
                lastFrameTraverse = frameTraverse;
            }
            else
            {
                frameTraverse = lastFrameTraverse; //movement defaults back to zero when not grounded
            }
            //print(frameTraverse);
            //print(lastFrameTraverse);

            moveSuspension(initialPosition, susTravIndex, frameTraverse, _susTrav);
            //end suspension movement
            if (_track.hasSteering)
            {
                Vector3 newSteeringAngle = initialSteeringAngles;
                newSteeringAngle[steeringIndex] += _track.steeringAngleSmoothed;
                _trackSteering.transform.localEulerAngles = newSteeringAngle;
            }
            
        }//end OnUpdate

        public void moveSuspension(Vector3 traverseStart, int index, float movement, Transform movedObject)
        {
            Vector3 tempVector = traverseStart;
            tempVector[index] -= movement;
            movedObject.localPosition = tempVector;
        }
    }//end modele
}//end class