using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalFoundries
{
    [KSPModule("ModuleMirror")]
    public class KFModuleMirror : PartModule
    {
        //public Transform leftObject;
        //public Transform rightObject;
        public string right = "right";
        public string left = "left";
        public string swap = "swap";
        [KSPField(isPersistant = true)]
        public string cloneSide;
        [KSPField(isPersistant = true)]
        public string flightSide;

        public KFModuleMirror clone;

        [KSPField]
        public string leftObjectName;
        [KSPField]
        public string rightObjectName;

        List<Transform> leftObject = new List<Transform>();
        List<Transform> rightObject = new List<Transform>();
        string[] rightList;
        string[] leftList;

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            leftList = leftObjectName.Split(new[] { ',', ' ', '|' }, StringSplitOptions.RemoveEmptyEntries); //Thanks, Mihara!
            rightList = rightObjectName.Split(new[] { ',', ' ', '|' }, StringSplitOptions.RemoveEmptyEntries);
            Debug.LogError(leftList[0] + " " + leftList.Count());
            Debug.LogError(rightList[0]+ " " + rightList.Count());

            //some defaults
            //if (leftObjectName == "")
              //  leftObjectName = "Left";
            //if (rightObjectName == "")
              //  leftObjectName = "Right";

            for (int i = 0; i < leftList.Count(); i++)
            {
                leftObject.Add (transform.Search(leftList[i]));
                print("iterated left " + i);
            }
            for (int i = 0; i < rightList.Count(); i++)
            {
                rightObject.Add(transform.Search(rightList[i]));
                print("iterated right " + i);
            }

            /*
            if (tr[i].name.Equals(leftObjectName, StringComparison.Ordinal))
            {
                print("Found left");
                leftObject = tr;
            }
            */

            if (HighLogic.LoadedSceneIsFlight)
            {
                //SetSide(flightSide); 
                print("Loaded scene is flight");
				if (Equals(flightSide, left))
                {
                    for (int i = 0; i < rightObject.Count(); i++)
                    {
						print(string.Format("Destroying Right object {0}", rightList[i]));
                        leftObject[i].gameObject.SetActive(true);
						UnityEngine.Object.Destroy(rightObject[i].gameObject);
                    }
                }
				if (Equals(flightSide, right))
                {
                    
                    for (int i = 0; i < leftObject.Count(); i++)
                    {
						print(string.Format("Destroying left object {0}", leftList[i]));
                        rightObject[i].gameObject.SetActive(true);
						UnityEngine.Object.Destroy(leftObject[i].gameObject);
                    }
                }
            }

            print("Loaded scene is editor");
            print(flightSide);

            FindClone();
			if (!Equals(clone, null))
            {
                print("Part is clone");
                //FindClone(); //make sure we have the clone. No harm in checking again
                SetSide(clone.cloneSide);
            }

			if (Equals(flightSide, "")) //check to see if we have a value in persistence
            {
                print("No flightSide value in persistence. Sertting default");
                //print(this.part.isClone);
                LeftSide();
            }
            else //flightSide has a value, so set it.
            {
                print("Setting value from persistence");
                SetSide(flightSide);
            }
        }//end OnStart

        /// <summary>Sets this side to left and clone to right.</summary>
        [KSPEvent(guiName = "Left", guiActive = false, guiActiveEditor = true)]
        public void LeftSide()
        {
            FindClone();
            SetSide(left);
            if (clone)
                clone.SetSide(right);
            }
        /// <summary>Sets this side to right and clone to left.</summary>
        [KSPEvent(guiName = "Right", guiActive = false, guiActiveEditor = true)]
        public void RightSide()
        {
            FindClone();
            SetSide(right);
            if (clone)
                clone.SetSide(left);
            }

        public void SetSide(string side) //accepts the string value
        {
			if (Equals(side, left))
            {
                for (int i = 0; i < leftList.Count(); i++)
                {
                    rightObject[i].gameObject.SetActive(false);
                    leftObject[i].gameObject.SetActive(true);
                }
                cloneSide = right;
                flightSide = side;
                Events["LeftSide"].active = false;
                Events["RightSide"].active = true;
            }
			if (Equals(side, right))
            {
                for (int i = 0; i < leftList.Count(); i++)
                {
                    rightObject[i].gameObject.SetActive(true);
                    leftObject[i].gameObject.SetActive(false);
                }
                cloneSide = left;
                flightSide = side;
                Events["LeftSide"].active = true;
                Events["RightSide"].active = false;
            }
        }

        public void FindClone()
        {
            foreach (Part potentialMaster in this.part.symmetryCounterparts) //search for parts that might be my symmetry counterpart
            {
				if (!Equals(potentialMaster, null)) //or we'll get a null-ref
                {
                    clone = potentialMaster.Modules.OfType<KFModuleMirror>().FirstOrDefault();
                    //print("found my clone");
                }
            }
        }
    }//end class
}//end namespace
