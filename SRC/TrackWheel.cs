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
        [KSPField(isPersistant=true)]
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

        //object types
        WheelCollider _wheelCollider;
        Transform _susTrav;
        Transform _wheel;
        Transform _trackSteering;
        ModuleTrack _track;

        //gloabl variables
        Vector3 initialTraverse;
        Vector3 initialSteeringAngles;
        Transform susStart;
        Vector3 wheelRotation;
        float lastTempTraverse;
        int susTravIndex = 1;
        int steeringIndex = 1;
        public int directionCorrector = 1;
        

        //OnStart
        public override void OnStart(PartModule.StartState state)
        {
            if (HighLogic.LoadedSceneIsGame)
            {
                foreach (WheelCollider wc in this.part.GetComponentsInChildren<WheelCollider>())
                {
                    if (wc.name.StartsWith(colliderName, StringComparison.Ordinal))
                    {
                        _wheelCollider = wc;
                        suspensionDistance = wc.suspensionDistance;
                    }
                }

            }
            print("TrackWheel Called");
            if (HighLogic.LoadedSceneIsEditor)
            {

            }
            
            if (HighLogic.LoadedSceneIsFlight)
            {
                //find names onjects in part
                this.part.force_activate();

                print("suspensionDistance is");
                print(suspensionDistance);

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


                _track = this.part.GetComponentInChildren<ModuleTrack>();

                susTravIndex = Extensions.SetAxisIndex(susTravAxis);
                steeringIndex = Extensions.SetAxisIndex(steeringAxis);

                initialTraverse = _susTrav.transform.localPosition;
                if (_track.hasSteering)
                {
                    initialSteeringAngles = _trackSteering.transform.localEulerAngles;
                    print(initialSteeringAngles);
                }

                if (useDirectionCorrector)
                    directionCorrector = _track.directionCorrector;
                else directionCorrector = 1;
                print(directionCorrector);

                wheelRotation = new Vector3(wheelRotationX, wheelRotationY, wheelRotationZ);
                //lastTempTraverse = initialTraverse[susTravIndex] - _wheelCollider.suspensionDistance;
                lastTempTraverse = susStart.localPosition[susTravIndex] - _wheelCollider.suspensionDistance;
            }
            //end find named objects
            base.OnStart(state);
        }//end OnStart
        //OnUpdate

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            _wheel.transform.Rotate(wheelRotation, _track.degreesPerTick * directionCorrector * rotationCorrection); //rotate wheel
            //suspension movement
            WheelHit hit;
            float tempFloat = 0;
            //Vector3 tempTraverse = initialTraverse;
            Vector3 tempTraverse = susStart.localPosition;
            bool grounded = _wheelCollider.GetGroundHit(out hit); //set up to pass out wheelhit coordinates
            _wheelCollider.suspensionDistance = suspensionDistance * _track.appliedRideHeight;

            if (grounded && !isSprocket) //is it on the ground
            {
                //tempTraverse[susTravIndex] -= Mathf.Clamp( ((-_wheelCollider.transform.InverseTransformPoint(hit.point).y + _track.raycastError) - _wheelCollider.radius), -_wheelCollider.suspensionDistance, _wheelCollider.suspensionDistance);// / wheelCollider.suspensionDistance; //out hit does not take wheel radius into account
                tempTraverse[susTravIndex] -= (-_wheelCollider.transform.InverseTransformPoint(hit.point).y + _track.raycastError) - _wheelCollider.radius;
                tempFloat = -_wheelCollider.transform.InverseTransformPoint(hit.point).y + _track.raycastError - _wheelCollider.radius;
                //print(tempFloat);
                lastTempTraverse = tempFloat;
                //lastTempTraverse = tempTraverse[susTravIndex];
                //print(tempTraverse);
            }
            else
            {
                tempFloat = lastTempTraverse;
                //tempTraverse[susTravIndex] = lastTempTraverse;

            } //movement defaults back to zero when not grounded
            //_susTrav.transform.localPosition = tempTraverse; //move the suspensioTraverse object
            Vector3 tempVector = susStart.localPosition;
            tempVector[susTravIndex] -= tempFloat;
            _susTrav.localPosition = tempVector;
            //print(_susTrav.localPosition);
            
            if (_track.hasSteering)
            {
                Vector3 newSteeringAngle = initialSteeringAngles;
                newSteeringAngle[steeringIndex] -= _track.steeringAngleSmoothed;
                _trackSteering.transform.localEulerAngles = newSteeringAngle;
            }
            //end suspension movement
        }//end OnUpdate
    }//end modele
}//end class