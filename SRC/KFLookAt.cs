using System;
using System.Collections;
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
        public float mirrorOffset = 0;
        [KSPField]
        public bool activeEditor = false;

        List<Transform> rotators = new List<Transform>();
        List<Transform> targets = new List<Transform>();

        string[] rotatorList;
        string[] targetList;

        Transform _target;
        Transform _rotator;
        Transform _mirrorObject;

        int objectCount = 0;

        bool countAgrees;

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            
            
            


            rotatorList = rotatorsName.Split(new[] { ',', ' ', '|' }, StringSplitOptions.RemoveEmptyEntries); //Thanks, Mihara!
            targetList = targetName.Split(new[] { ',', ' ', '|' }, StringSplitOptions.RemoveEmptyEntries);

            

            if (HighLogic.LoadedSceneIsEditor && activeEditor)
            {
                SetupObjects();
                if(countAgrees)
                    StartCoroutine(Rotator());
            }
            if (HighLogic.LoadedSceneIsFlight)
            {
                SetupObjects();
                if (countAgrees)
                    StartCoroutine(Rotator());
            }
        }
        public void SetupObjects()
        {
            //_rotator = transform.Search(rotatorsName);
            //_target = transform.Search(targetName);
            //_mirrorObject = transform.Search(mirrorObjectName);
            print("setup objects");
            for (int i = 0; i < rotatorList.Count(); i++)
            {
                rotators.Clear();
                rotators.Add(transform.SearchStartsWith(rotatorList[i]));
                print("iterated rotators " + rotatorList.Count());
            }
            for (int i = 0; i < targetList.Count(); i++)
            {
                targets.Clear();
                targets.Add(transform.SearchStartsWith(targetList[i]));
                print("iterated targets " + targetList.Count());
            }
            objectCount = rotators.Count();

            if(objectCount == targets.Count())
                countAgrees = true;

            /*
            foreach (Transform tr in this.part.GetComponentsInChildren<Transform>())
            {
                if (tr.name.StartsWith(rotatorsName, StringComparison.Ordinal))
                {
                    _rotator = tr;
                }
                if (tr.name.StartsWith(targetName, StringComparison.Ordinal))
                {
                    _target = tr;
                }
                if (tr.name.StartsWith(mirrorObjectName, StringComparison.Ordinal))
                {
                    _mirrorObject = tr;
                }
            }
            */

        }

        IEnumerator Rotator()
        {
            while (true)
            {
                for (int i = 0; i < objectCount; i++)
                {
                    rotators[i].LookAt(targets[i], rotators[i].transform.up);
                }

                /*
                  var relativeUp = _target.TransformDirection(Vector3.forward);
                  var relativePos = _target.position - _rotator.transform.position;
                
                _rotator.transform.rotation = Quaternion.LookRotation(relativePos, relativeUp);
                 * * */
                yield return null;
            }
        }

        public void Update()
        {
            base.OnUpdate();
        }
    }
}
