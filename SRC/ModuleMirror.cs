using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalFoundries
{
    [KSPModule("ModuleMirror")]
    public class ModuleMirror : PartModule
    {

        public Transform leftObject;
        public Transform rightObject;
        public string right = "right";
        public string left = "left";
        public string swap = "swap";


        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            //if (this.part.isMirrored)
            //{
                //print("this part is mirrored");
            //}
            foreach (Transform tr in this.part.GetComponentsInChildren<Transform>())
            {
                if (tr.name.Equals("Left", StringComparison.Ordinal))
                {
                    //print("Found left");
                    leftObject = tr;
                }

                if (tr.name.Equals("Right", StringComparison.Ordinal))
                {
                    //print("Found right");
                    rightObject = tr;
                }
            }

            if (HighLogic.LoadedSceneIsEditor)
            {
                part.OnEditorAttach += onEditorAttach;
                part.OnEditorDetach += onEditorDetach;
                
                if(part.isClone)
                {
                    onEditorAttach();
                }

                print("isClone");
                print(this.part.isClone); 
 
                if (!part.isClone)
                {
                    SetSide(right);
                }
            }//end editor
        }//end OnStart
        public virtual void onEditorAttach()
        {
            print("onEditorAttach called");
            //print(part.transform.forward);
            //print(part.GetReferenceTransform());
            float dot = Vector3.Dot(this.part.transform.forward, part.GetReferenceTransform().up); // up is forward
            if (dot < 0) // below 0 means the engine is on the left side of the craft
            {
                //print("part is left");
                SetSide(left);
            }
            else
            {
                //print("part is right");
                SetSide(right);
            }
        }
        public virtual void onEditorDetach()
        {
            print("onEditorDetach called");
            //print(part.transform.forward);
            //print(part.GetReferenceTransform());
            float dot = Vector3.Dot(this.part.transform.forward, part.GetReferenceTransform().up); // up is forward
            if (dot < 0) // below 0 means the engine is on the left side of the craft
            {
                //print("part is left");
                SetSide(left); 
            }
            else
            {
                //print("part is right");
                SetSide(right);
            }
        }
        public void SetSide(string side)
        {
            if (side == left)
            {
                rightObject.gameObject.SetActive(false);
                leftObject.gameObject.SetActive(true); 
            }
            if (side == right)
            {
                rightObject.gameObject.SetActive(true);
                leftObject.gameObject.SetActive(false);
            }
            if (side == swap)
            {
                rightObject.gameObject.SetActive(true);
                leftObject.gameObject.SetActive(false);
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            
            bool swap = Input.GetKey(KeyCode.Hash);
            
        }

    }//end class
}//end namespace
