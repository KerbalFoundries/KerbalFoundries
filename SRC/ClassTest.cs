using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace KerbalFoundries
{
    //[KSPModule("ClassTest")]   commented this line out, I don't use it in my own mods so I'm not sure what it does.
    public class ClassTest : PartModule  //made the module public, not sure it is necessary but it makes sure calls from other places work fine
    {
        public float testFloat;
        public ConfigNode ourTestNode;
        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            if (HighLogic.LoadedSceneIsFlight)
                testFloat = 2.55f;
            print("ClassTest called");
            ourTestNode = new ConfigNode(); //define your SubNode class as normal, this example assumes it has a "public string name;" field defined.
        }
        //how do I tab over in the forum edit box anyway?
        public override void OnLoad(ConfigNode node)
        {
            ConfigNode subNodeLoad = node.GetNode("SUBNODE"); //this example assumes a single SUBNODE, if you have (or may have) multiple SUBNODE, you use node.GetNodes to return a list of SUBNODE.
            ourTestNode.name = node.GetValue("name"); //
            print("SUBNODE loaded named: " + ourTestNode.name);
        }
    }
}