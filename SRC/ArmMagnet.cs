using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalFoundries
{
    public class ArmMagnet : PartModule
    {
        [KSPField]
        public string hookObjectName;
        [KSPField]
        public int layerMask = 0;
        [KSPField(guiName = "Ray", guiActive = true)]
        public string ravInfo;

        GameObject _hook;
        GameObject _target;
        Rigidbody _rb;
        Rigidbody _targetRb;
        ConfigurableJoint _joint;
        bool isReady;


        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            if (HighLogic.LoadedSceneIsFlight)
            {
                Debug.LogError(hookObjectName);
                _hook = transform.Search(hookObjectName).gameObject;
                isReady = true;
                Debug.LogError("Test Arm started");
            }
            
        }

        public void FixedUpdate()
        {
            if (!isReady)
                return;
            //Debug.Log("running Fixedupdate");
            Ray ray = new Ray(_hook.transform.position, _hook.transform.forward);
            RaycastHit hit;

            int tempLayerMask = ~layerMask;

            if (Physics.Raycast(ray, out hit, 0.5f, tempLayerMask))
            {
                ravInfo = hit.collider.gameObject.name.ToString();
                try
                {
                    _target = hit.collider.gameObject;
                    _targetRb = (hit.rigidbody);
                }
                catch (NullReferenceException) { }
            }
            else
            {
                ravInfo = "Nothing";
                _target = null;
                //_rb = null;
            }
        }

        [KSPEvent(guiActive = true, guiName = "Grab", active = true, guiActiveUnfocused = true, unfocusedRange = 40f)]
        public void Grab()
        {
            if (_target != null)
            {
                _rb = _hook.AddComponent<Rigidbody>();
                _rb.isKinematic = true;
                _rb.mass = 0.1f;
                _rb.useGravity = false;
                //_rb.constraints = RigidbodyConstraints.FreezeAll;

                _joint = _hook.AddComponent<ConfigurableJoint>();

                _joint.xMotion = ConfigurableJointMotion.Locked;
                _joint.yMotion = ConfigurableJointMotion.Locked;
                _joint.zMotion = ConfigurableJointMotion.Locked;

                _joint.angularXMotion = ConfigurableJointMotion.Locked;
                _joint.angularYMotion = ConfigurableJointMotion.Locked;
                _joint.angularZMotion = ConfigurableJointMotion.Locked;

                _joint.connectedBody = _targetRb; 

            }
        }
    }
}
