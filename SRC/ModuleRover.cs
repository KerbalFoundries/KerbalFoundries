using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalFoundries
{
    [KSPModule("ModeleRover")]
    public class ModuleRover : PartModule
    {
        [KSPField(isPersistant = false, guiActive = true, guiName = "Dot.X", guiFormat = "F6")]
        public float dotx; //debug only
        [KSPField(isPersistant = false, guiActive = true, guiName = "Dot.Y", guiFormat = "F6")]
        public float doty; //debug only
        [KSPField(isPersistant = false, guiActive = true, guiName = "Dot.Z", guiFormat = "F6")]
        public float dotz; //debug only

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            if (HighLogic.LoadedSceneIsFlight)
            {

            }
        }


    }
}
