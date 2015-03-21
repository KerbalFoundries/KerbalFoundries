using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalFoundries
{
    public class OrientationMarker : PartModule
    {
		//Log prefix to more easily identify this mod's log entries.
		public const string logprefix = "[KF - OrientationMarker]: ";
		
        [KSPField]
        public string markerName;
        public Transform marker;
        /*
        protected override void onPartLoad()
        { 
            base.onPartLoad();
            Debug.LogWarning("OrientationMarker");
            marker = transform.Search(markerName);
            if (markerName != null)
            {
                marker.gameObject.SetActive(false);
                Debug.LogWarning(string.Format("{0}Marker destroyed", logprefix));
            }
            else
                Debug.LogWarning(string.Format("{0}Marker not found", logprefix));
        }
        */

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            marker = this.part. transform.Search(markerName);
            if (markerName != null && HighLogic.LoadedSceneIsFlight)
            {
                GameObject.Destroy(marker.gameObject);
                Debug.LogWarning(string.Format("{0}Marker destroyed", logprefix));
            }
            else
				Debug.LogWarning(string.Format("{0}Marker not found", logprefix));
        }

    }
}
