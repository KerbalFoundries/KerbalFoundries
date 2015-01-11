using System;
using System.Collections;
using System.Linq;
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
        public float mirrorOffset = 0;
        [KSPField]
        public bool activeEditor = false;

        Transform _target;
        Transform _rotator;
        Transform _mirrorObject;

        bool invert = false;

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            if (HighLogic.LoadedSceneIsEditor && activeEditor)
            {
                SetupObjects();
                StartCoroutine(Rotator());
            }
            if (HighLogic.LoadedSceneIsFlight)
            {
                SetupObjects();
                StartCoroutine(Rotator());
            }
        }
        public void SetupObjects()
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

        IEnumerator Rotator()
        {
            while (true)
            {
                _rotator.LookAt(_target.position, this.part.transform.forward);
                if (invert == true)
                {
                    _rotator.Rotate(Vector3.right, 180 - mirrorOffset);
                }
                yield return null;
            }
        }

        public void Update()
        {
            base.OnUpdate();
        }
    }
}
