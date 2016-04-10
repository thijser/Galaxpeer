using UnityEngine;
using Galaxpeer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class MobileEntityGameObject : MonoBehaviour {
	public Guid Uuid;
	public string id;
	public int Health;

	void Start () {
		id = Uuid.ToString ();
	}

	public MobileEntity Entity {
		get {
			UnityUnityInterface UUI = (UnityUnityInterface)UnityInterfaceInterfaceManager.InstanceUnintyInterface;
			return UUI.GetEntity (Uuid);
		}
	}
	
	// Update is called once per frame
	void Update () {
		var me = Entity;
		if (me != null) {
			Health = me.Health;
			transform.position = Conversion.ToUnity (me.Location);
			transform.rotation = Conversion.ToUnity (me.Rotation);
		}
	}
}
