using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalFoundries
{
    public class KFTrackSurface : PartModule
    {
        GameObject _trackSurface;
        KFModuleWheel _track;
        [KSPField]
        public float trackLength = 10;

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            print(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);

            foreach (SkinnedMeshRenderer Track in this.part.GetComponentsInChildren<SkinnedMeshRenderer>()) //this is the track
            {
                _trackSurface = Track.gameObject;
            }

            _track = this.part.GetComponentInChildren<KFModuleWheel>();

        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            float degreesPerTick = (_track.averageTrackRPM / 60) * Time.deltaTime * 360; //calculate how many degrees to rotate the wheel
            float distanceTravelled = (float)((_track.averageTrackRPM * 2 * Math.PI) / 60) * Time.deltaTime; //calculate how far the track will need to move
            Material trackMaterial = _trackSurface.renderer.material;    //set things up for changing the texture offset on the track
            Vector2 textureOffset = trackMaterial.mainTextureOffset;
            textureOffset = textureOffset + new Vector2(-distanceTravelled / trackLength, 0); //tracklength is used to fine tune the speed of movement.
            trackMaterial.SetTextureOffset("_MainTex", textureOffset);
            trackMaterial.SetTextureOffset("_BumpMap", textureOffset);
        }
    }
}
