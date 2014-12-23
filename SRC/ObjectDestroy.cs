using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalFoundries
{
    public class ObjectDestroy : PartModule
    {
        [KSPField]
        public string objectName;

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            Transform destroyedObject = transform.Search(objectName);
            if (destroyedObject != null)
            {
                GameObject.Destroy(destroyedObject.gameObject);
                //boundsDestroyed = true; //remove the bounds object to let the wheel colliders take over
                print("destroying " + objectName);
            }
            else
                Debug.LogWarning("could not find object named " + objectName); 
        }
    }
}
