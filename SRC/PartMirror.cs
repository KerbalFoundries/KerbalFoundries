using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalFoundries
{
    public class PartMirror : PartModule
    {

        public Transform leftObject;
        public Transform rightObject;
        public string right = "right";
        public string left = "left";
        public string swap = "swap";
        [KSPField(isPersistant = true)]
        public string flightSide;

        [KSPField]
        public string leftObjectName;
        [KSPField]
        public string rightObjectName;

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            if (flightSide == "") //check to see if we have a value in persistence
            {
                print("No flightSide value in persistence. Setting default");
                //print(this.part.isClone);
                SetSide(left);
            }
            else //flightSide has a value, so set it.
            {
                print("Setting value from persistence");
                SetSide(flightSide);
            }

            foreach (Transform tr in this.part.GetComponentsInChildren<Transform>())
            {
                if (tr.name.Equals(leftObjectName, StringComparison.Ordinal))
                {
                    //print("Found left"); 
                    leftObject = tr;
                }

                if (tr.name.Equals(rightObjectName, StringComparison.Ordinal))
                {
                    //print("Found right");   
                    rightObject = tr;
                }
            }

            if (HighLogic.LoadedSceneIsFlight)
            {
                //SetSide(flightSide); 
                print("Loaded scene is flight");
                if (flightSide == left)
                {
                    print("Destroying Right object");
                    SetSide(left);
                    GameObject.Destroy(rightObject.gameObject);
                }
                if (flightSide == right)
                {
                    print("Destroying left object");
                    SetSide(right);
                    GameObject.Destroy(leftObject.gameObject);
                }
                print("Side is");
                print(flightSide);
            }

        }//end OnStart


        public void SetSide(string side) //accepts the string value
        {
            if (side == left)
            {
                rightObject.gameObject.SetActive(false);
                leftObject.gameObject.SetActive(true);

                flightSide = side;

            }
            if (side == right)
            {
                rightObject.gameObject.SetActive(true);
                leftObject.gameObject.SetActive(false);

                flightSide = side;
            }
        }

    }//end class
}//end namespace
