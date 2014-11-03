using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalFoundries
{
    public static class WheelUtils
    {
        public static int GetCorrector(Vector3 transformVector, Transform referenceVector, int directionIndex) // takes a vector (usually from a parts axis) and a transform, plus an index giving which axis to   
        {                                                                                               // use for the scalar product of the two. Returns a value of -1 or 1, depending on whether the product is positive or negative.
            int corrector = 1;
            float dot = 0;

            if (directionIndex == 0)
            {
                dot = Vector3.Dot(transformVector, referenceVector.right); // up is forward

            }
            if (directionIndex == 1)
            {
                dot = Vector3.Dot(transformVector, referenceVector.up); // up is forward
            }
            if (directionIndex == 2)
            {
                dot = Vector3.Dot(transformVector, referenceVector.forward); // up is forward
            }

            //print(dot);

            if (dot < 0) // below 0 means the engine is on the left side of the craft
            {
                corrector = -1;
            }
            else
            {
                corrector = 1;
            }
            return corrector;
        }

        public static int GetRefAxis(Vector3 refDirection, Transform refTransform) //takes a vector 3 derived from the axis of the parts transform (typically), and the transform of the part to compare to (usually the root part)
        {                                                                   // uses scalar products to determine which axis is closest to the axis specified in refDirection, return an index value 0 = X, 1 = Y, 2 = Z
            //orgpos = this.part.orgPos; //debugguing
            float dotx = Math.Abs(Vector3.Dot(refDirection, refTransform.right)); // up is forward
            //print(dotx); //debugging
            float doty = Math.Abs(Vector3.Dot(refDirection, refTransform.up));
            //print(doty); //debugging
            float dotz = Math.Abs(Vector3.Dot(refDirection, refTransform.forward));
            //print(dotz); //debugging

            int orientationIndex = 0;

            if (dotx > doty && dotx > dotz)
            {
                //print("root part mounted right");
                orientationIndex = 0;
            }
            if (doty > dotx && doty > dotz)
            {
                //print("root part mounted forward");
                orientationIndex = 1;
            }
            if (dotz > doty && dotz > dotx)
            {
                //print("root part mounted up");
                orientationIndex = 2;
            }
            /*
            if (referenceDirection == 0)
            {
                referenceTranformVector.x = Math.Abs(referenceTranformVector.x);
            }
             * */
            return orientationIndex;
        }

        public static float SetupRatios(int refIndex, Part thisPart, Vessel thisVessel, int groupNumber)      // Determines how much this wheel should be steering according to its position in the craft. Returns a value -1 to 1.
        {
            float myPosition = thisPart.orgPos[refIndex];
            float maxPos = thisPart.orgPos[refIndex];
            float minPos = thisPart.orgPos[refIndex];
            float ratio = 1;
            foreach (KFModuleWheel st in thisVessel.FindPartModulesImplementing<KFModuleWheel>()) //scan vessel to find fore or rearmost wheel. 
            {
                if (st.groupNumber == groupNumber && groupNumber != 0)
                {
                    float otherPosition = myPosition;
                    otherPosition = st.part.orgPos[refIndex];

                    if ((otherPosition + 1000) >= (maxPos + 1000)) //dodgy hack. Make sure all values are positive or we struggle to evaluate < or >
                        maxPos = otherPosition; //Store transform y value

                    if ((otherPosition + 1000) <= (minPos + 1000))
                        minPos = otherPosition; //Store transform y value
                }
            }

            float minToMax = maxPos - minPos;
            float midPoint = minToMax / 2;
            float offset = (maxPos + minPos) / 2;
            float myAdjustedPosition = myPosition - offset;

            ratio = myAdjustedPosition / midPoint;

            if (ratio == 0 || float.IsNaN(ratio)) //check is we managed to evaluate to zero or infinity somehow. Happens with less than three wheels, or all wheels mounted at the same position.
                ratio = 1;
            //print("ratio"); //Debugging
            //print(ratio);
            return ratio;
        }
    } //end class
}
