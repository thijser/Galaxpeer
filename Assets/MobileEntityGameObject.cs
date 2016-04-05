﻿using UnityEngine;
using Galaxpeer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class MobileEntityGameObject : MonoBehaviour {
	public Guid Uuid;
	public string id;

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
		//Transform t=transform;
		if (me.Type != MobileEntity.EntityType.Player) {
			Debug.Log (Conversion.ToUnity(me.Location));
		}
		transform.position = Conversion.ToUnity (me.Location);
		transform.rotation = Conversion.ToUnity (me.Rotation);
	}
}
