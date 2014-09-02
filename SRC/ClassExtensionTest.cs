using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalFoundries
{
    public class ClassExtensionTest : ClassTest
    {
        [KSPField]
        public float extensionFloat;

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            if (HighLogic.LoadedSceneIsFlight)
            {
                print("ClassExtensionTest called"); 
                print(testFloat);
                print(extensionFloat);
            }
        }
    }
}
