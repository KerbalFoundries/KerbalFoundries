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

        public Transform leftObject;
        public Transform rightObject;
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

        [KSPField]
        public string leftTargetName;
        [KSPField]
        public string leftRotatorsName;

        [KSPField]
        public string rightTargetName;
        [KSPField]
        public string rightRotatorsName;

		//Log prefix to more easily identify this mod's log entries.
		public const string logprefix = "[KF - KFModuleMirror]: ";

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

			if (Equals(leftObjectName, string.Empty))
                leftObjectName = "Left";
			if (Equals(rightObjectName, string.Empty))
                //leftObjectName = "Right"; // shouldn't this be "rightObjectName" instead?
            	rightObjectName = "Right";

            foreach (Transform tr in this.part.GetComponentsInChildren<Transform>())
            {
                if (tr.name.Equals(leftObjectName, StringComparison.Ordinal))
                {
					print(string.Format("{0}Found left", logprefix));
                    leftObject = tr;
                }

                if (tr.name.Equals(rightObjectName, StringComparison.Ordinal))
                {
					print(string.Format("{0}Found right", logprefix));
                    rightObject = tr;
                }
            }

            if (HighLogic.LoadedSceneIsFlight)
            {
                //SetSide(flightSide); 
				print(string.Format("{0}Loaded scene is flight", logprefix));
				if (Equals(flightSide, left))
                {
					print(string.Format("{0}Destroying Right object", logprefix));
                    leftObject.gameObject.SetActive(true);
                    GameObject.Destroy(rightObject.gameObject);
                    //this.part.AddModule("FXModuleLookAtConstraint");
                    //FXModuleLookAtConstraint _fx = this.part.GetComponentInChildren<FXModuleLookAtConstraint>();

                }

                if (flightSide == right)
                {
					print(string.Format("{0}Destroying left object", logprefix)); 
                    rightObject.gameObject.SetActive(true);
                    GameObject.Destroy(leftObject.gameObject);
                }
            }

			print(string.Format("{0}Loaded scene is editor", logprefix));
            print(flightSide);

            FindClone();
            if (clone != null)
            {
				print(string.Format("{0}Part is clone", logprefix));
                //FindClone(); //make sure we have the clone. No harm in checking again
                SetSide(clone.cloneSide);
            }

            if (flightSide == "") //check to see if we have a value in persistence
            {
				print(string.Format("{0}No flightSide value in persistence. Sertting default", logprefix));
                //print(this.part.isClone);
                LeftSide();
            }
            else //flightSide has a value, so set it.
            {
				print(string.Format("{0}Setting value from persistence", logprefix));
                SetSide(flightSide);
            }


        }//end OnStart

        [KSPEvent(guiName = "Left", guiActive = false, guiActiveEditor = true)]
        public void LeftSide() //sets this side to left and clone to right
        {
            FindClone();
            SetSide(left);

            if (clone)
            {
                clone.SetSide(right);
            }
        }
        [KSPEvent(guiName = "Right", guiActive = false, guiActiveEditor = true)]
        public void RightSide()
        {
            FindClone();
            SetSide(right);

            if (clone)
            {
                clone.SetSide(left);
            }
        }

        public void SetSide(string side) //accepts the string value
        {
			if (Equals(side, left))
            {
                rightObject.gameObject.SetActive(false);
                leftObject.gameObject.SetActive(true);
                cloneSide = right;
                flightSide = side;
                Events["LeftSide"].active = false;
                Events["RightSide"].active = true;
            }
			if (Equals(side, right))
            {
                rightObject.gameObject.SetActive(true);
                leftObject.gameObject.SetActive(false);
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
                if (potentialMaster != null) //or we'll get a null-ref
                {
                    clone = potentialMaster.Modules.OfType<KFModuleMirror>().FirstOrDefault();
					//print(string.Format("{0}Found my clone", logprefix));
                }
            }
        }
    }//end class
}//end namespace
