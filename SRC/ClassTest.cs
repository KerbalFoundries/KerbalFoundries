using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class DebugPart: Part
{

// Called when part disconnects from vessel due to crash etc. - seems to be called first
// Also called when part is lowest part of a stack above a decoupler
protected override void onDisconnect()
{
print("DBG: onDisconnect");
base.onDisconnect();
}

// Called in VAB after onPartAttach() or any update of vessel in VAB
// Also called after onPack()
public override void onBackup()
{
print("DBG: onBackup");
base.onBackup();
}



// Called continually when part is active anywhere in VAB, after onPartStart()
protected override void onEditorUpdate()
{
//print("DBG: onEditorUpdate");
base.onEditorUpdate();
}

// Called in VAB when removing a part after onPartDelete()
// or in flight scene when destroying a part after onPartExplode()
// also after part goes out of range (&gt;2.5km) of focussed ship
protected override void onPartDestroy()
{
print("DBG: onPartDestroy");
base.onPartDestroy();
}

// Does not seem to be reliably called - mostly when decoupler explodes
protected override void onDecouple(float breakForce)
{
print("DBG: onDecouple(" + breakForce + ")");
base.onDecouple(breakForce);
}

// Called at beginning of flight scene after onPartStart()
// also when part comes in range of focussed ship (&lt;2.5km) after onPartStart()
protected override void onFlightStart()
{
print("DBG: onFlightStart");
base.onFlightStart();
}

// Called for launchpad start after onFlightStart()
protected override void onFlightStartAtLaunchPad()
{
print("DBG: onFlightStartAtLaunchPad");
base.onFlightStartAtLaunchPad();
}






// Called when any vessel control input occurs, even automatic
// may be called during part placement before flight scene is properly setup
protected override void onCtrlUpd(FlightCtrlState s)
{
//print("DBG: onCtrlUpd(" + s + ")");
base.onCtrlUpd(s);
}

// Called when game is paused (ESC)
protected override void onGamePause()
{
print("DBG: onGamePause");
base.onGamePause();
}

// Called when game in unpaused
protected override void onGameResume()
{
print("DBG: onGameResume");
base.onGameResume();
}

// Called when part soft-lands on water
protected override void onPartSplashdown()
{
print("DBG: onPartSplashdown");
}

// Called when part goes on rails (&gt;500m from focussed ship)
protected override void onPack()
{
print("DBG: onPack");
base.onPack();
}

// Called when part goes off-rails
protected override void onUnpack()
{
print("DBG: onUnpack");
base.onUnpack();
}

// Does not seem to be called
protected override void onPartTouchdown()
{
print("DBG: onPartTouchdown");
base.onPartTouchdown();
}

// Called in VAB when deleting a part, just before onPartDestroy()
protected override void onPartDelete()
{
print("DBG: onPartDelete");
base.onPartDelete();
}

// Called in VAB when attaching to another part - "parent" gives attached part
protected override void onPartAttach(Part parent)
{
print("DBG: onPartAttach(" + parent + ")");
base.onPartAttach(parent);
}

// called in VAB when detaching part from vessel
protected override void onPartDetach()
{
print("DBG: onPartDetach");
base.onPartDetach();
}


// Initial call in VAB when picking up part
// Also called when part comes into range of focussed ship (&lt;2.5km)
// And at initial part loading at program start
protected override void onPartAwake()
{
print("DBG: onPartAwake");
base.onPartAwake();
}

// Called when part is deactivated after onDisconnect()
protected override void onPartDeactivate()
{
print("DBG: onPartDeactivate");
base.onPartDeactivate();
}

// Called after explosion of part due to crash etc.
// Seems to be after explosion effects are applied
protected override void onPartExplode()
{
print("DBG: onPartExplode");
base.onPartExplode();
}

// Called in VAB after onPartAwake()
// Also in flight scene on vessel placement just before onFlightStart()
// and when part comes into range of focussed ship (&lt;2.5km)
protected override void onPartStart()
{
print("DBG: onPartStart");
base.onPartStart();
}

// Called during initial part load at start of game
protected override void onPartLoad()
{
print("DBG: onPartLoad");
base.onPartLoad();
}

// Does not seem to be called
protected override void onPartLiftOff()
{
print("DBG: onPartLiftOff");
base.onPartLiftOff();
}

// called regularly during flight scene if on focussed vessel - after each frame render?
protected override void onPartUpdate()
{
//print("DBG: onPartUpdate");
base.onPartUpdate();
}


// called continuously during flight scene if on focussed vessel
protected override void onPartFixedUpdate()
{
//print("DBG: onPartFixedUpdate");
base.onPartFixedUpdate();
}

// called regularly during flight scene if in active stage - after each frame render?
protected override void onActiveUpdate()
{
//print("DBG: onActiveUpdate");
base.onActiveUpdate();
}

// called continously during flight scene if in active stage
protected override void onActiveFixedUpdate()
{
//print("DBG: onActiveFixedUpdate");
base.onActiveFixedUpdate();
}

// called for parts connected to something else after onPack()
protected override void onJointDisable()
{
print("DBG: onJointDisable");
base.onJointDisable();
}

// called for parts connected to something else after onUnpack()
protected override void onJointReset()
{
print("DBG: onJointReset");
base.onJointReset();
}

// called when stage part is on becomes active
protected override bool onPartActivate()
{
print("DBG: onPartActivate");
return base.onPartActivate();
}


}