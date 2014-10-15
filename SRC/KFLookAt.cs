using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalFoundries
{
    public class KFLookAt : PartModule
    {
        [KSPField]
        public string targetName;
        [KSPField]
        public string rotatorsName;
        [KSPField]
        public string mirrorObjectName;
        [KSPField]
        public float mirrorOffset;

        Transform _target;
        Transform _rotator;
        Transform _mirrorObject;

        bool invert = false;

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
             
            if (HighLogic.LoadedSceneIsFlight)
            {
                _rotator = transform.Search(rotatorsName);
                _target = transform.Search(targetName);
                _mirrorObject = transform.Search(mirrorObjectName);

                if (_mirrorObject.transform.localScale.x < 0 || _mirrorObject.transform.localScale.y < 0 || _mirrorObject.transform.localScale.z < 0)
                {
                    invert = true;
                }
                else
                    invert = false;
            } 
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            _rotator.LookAt(_target.position, this.part.transform.forward);
            if (invert == true)
            {
                _rotator.Rotate(Vector3.right, 180 - mirrorOffset);
            }

        }
    }
}
