using System;
using System.Linq;

namespace KerbalFoundries
{
    class ModulePropeller : PartModule
    {
        KFModuleWheel master;

		//Log prefix to more easily identify this mod's log entries.
		public const string logprefix = "[KF - ModulePropeller]: ";
        
        public override string GetInfo ()
		{
			return "This part is capable of propelling the vessel through water.";
		}
        
        [KSPField]
        public float propellerForce = 5;

        public override void OnStart(PartModule.StartState state)
        {
        	print(string.Format("{0}ModulePropeller called", logprefix));
            base.OnStart(state);
            if (HighLogic.LoadedSceneIsFlight) 
            {
                master = this.part.GetComponentInChildren<KFModuleWheel>();
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (this.part.Splashed)
            {
                float forwardPropellorForce = master.directionCorrector * propellerForce * this.vessel.ctrlState.wheelThrottle;
                float turningPropellorForce = (propellerForce / 3) * this.vessel.ctrlState.wheelSteer;
                part.rigidbody.AddForce(this.part.GetReferenceTransform().forward * (forwardPropellorForce - turningPropellorForce));
            }
        }
    }
} 
