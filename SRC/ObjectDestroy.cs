using System;
using System.Linq;
using UnityEngine;

namespace KerbalFoundries
{
    public class ObjectDestroy : PartModule
    {
		//Log prefix to more easily identify this mod's log entries.
		public const string logprefix = "[KF - ObjectDestroy]: ";
		
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
				print(string.Format("{0}Destroying {1}", logprefix, objectName));
            }
            else
				Debug.LogWarning(string.Format("{0}Could not find object named {1}", logprefix, objectName));
        }
    }
}
