using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalFoundries
{
    public class KFMeccanumWheel :PartModule
    {
        [KSPField]
        public string wheelName;
        [KSPField]
        public float rotationSpeed;
        [KSPField]
        public Vector3 rotationAxis = new Vector3(0,0,1);
        Transform _wheel;
        bool isReady;
        KFModuleWheel _ModuleWheel;

		//Log prefix to more easily identify this mod's log entries.
		public const string logprefix = "[KF - KFMeccanumWheel]: ";

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            if (HighLogic.LoadedSceneIsFlight)
            {
                foreach (Transform tr in this.part.GetComponentsInChildren<Transform>())
                {
                    if (tr.name.StartsWith(wheelName, StringComparison.Ordinal))
                    {
                        _wheel = tr;
                        isReady = true;
						print(string.Format("{0}Found Wheel transform", logprefix));
                    }
                }
            }
            _ModuleWheel = this.part.GetComponentInChildren<KFModuleWheel>();

        }
        public void FixedUpdate()
        {
            if (!isReady)
                return;
            float _steeringRatio = _ModuleWheel.steeringRatio;
            var _rotate = (this.vessel.ctrlState.wheelThrottle * - _ModuleWheel.directionCorrector) + this.vessel.ctrlState.wheelSteer;
            _wheel.transform.Rotate(rotationAxis, _rotate * rotationSpeed);
        }

    }
}
