using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalFoundries
{
    [KSPModule("APUController")]
    class APUController : PartModule
    {
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Throttle"), UI_FloatRange(minValue = 0, maxValue = 100, stepIncrement = 5f)]
        public float throttleSetting = 50;        //this is what's tweaked by the line above
        public ModuleEngines thisEngine;

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            FindEngine(); 
            if (HighLogic.LoadedSceneIsFlight)
            {
                this.part.force_activate();
            }
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            
            thisEngine.currentThrottle = throttleSetting / 100;
                        
        }

        public void FindEngine()
        {
            foreach (ModuleEngines me in this.part.GetComponentsInChildren<ModuleEngines>())
            {
                print("Found an engine");
                thisEngine = me;
            }
        }

        [KSPAction("APU + output")]
        public void IncreaseAPU(KSPActionParam param)
        {
            if (throttleSetting < 100)
            {
                throttleSetting += 5f;
                print("Increasing APU Output");
            }
        }//End Retract
        [KSPAction("APU - output")]
        public void DecreaseAPU(KSPActionParam param)
        {
            if (throttleSetting > 0)
            {
                throttleSetting -= 5f;
                print("Decreasing APU Output");
            }
        }//End Retract
        [KSPAction("APU Shutdown")]
        public void ShutdownAPU(KSPActionParam param)
        {
                throttleSetting = 0f;
                print("Shutting down APU");
        }//End Retract

    }
}
