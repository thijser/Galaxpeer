using UnityEngine;
using Galaxpeer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class MobileEntityGameObject : MonoBehaviour {
	public Guid Uuid;
	
	// Update is called once per frame
	void Update () {
		UnityUnityInterface UUI = (UnityUnityInterface)UnityInterfaceInterfaceManager.InstanceUnintyInterface;
		MobileEntity me = UUI.GetEntity (Uuid);
		Transform t=transform;
		var pos = me.Location;
		t.position = new UnityEngine.Vector3(pos.X,pos.Y,pos.Z);
		var rot = me.Rotation;
		t.rotation = new Quaternion (rot.X, rot.Y, rot.Z, rot.W);
	}
}
