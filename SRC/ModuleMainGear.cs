using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalFoundries
{
    [KSPModule("ModuleLMainGear")]
    public class ModuleMainGear : PartModule
    {
        public bool boundsDestroyed;
        [KSPField(isPersistant = true)]
        public bool brakesApplied;
        public override void OnStart(PartModule.StartState start)  //when started
        {
            print("ModuleLMainGear called");
            base.OnStart(start);
            if (HighLogic.LoadedSceneIsEditor)
            {

            }

            if (HighLogic.LoadedSceneIsFlight)
            {
                foreach (WheelCollider wc in this.part.GetComponentsInChildren<WheelCollider>())
                {
                    wc.enabled = true;
                }
            }
            Transform bounds = transform.Search("Bounds");
            if (bounds != null)
            {
                GameObject.Destroy(bounds.gameObject);
                //boundsDestroyed = true; //remove the bounds object to left the wheel colliders take over
                print("destroying Bounds");
            }
        }
    }
}
