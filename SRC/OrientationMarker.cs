using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalFoundries
{
    public class OrientationMarker : PartModule
    {
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
                Debug.LogWarning("Marker deactivated");
            }
            else
                Debug.LogError("Marker not Found");
        }
        */

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            marker = this.part. transform.Search(markerName);
            if (markerName != null && HighLogic.LoadedSceneIsFlight)
            {
                GameObject.Destroy(marker.gameObject);
                Debug.LogWarning("Marker destroyed");
            }
            else
                Debug.LogWarning("Marker not found");
        }

    }
}
