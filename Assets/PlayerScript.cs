using UnityEngine;
using System;
using System.Collections;
using Galaxpeer;

public class PlayerScript : MonoBehaviour {


	// Use this for initialization
	void Start () {
		UnityUnityInterface UUI = (UnityUnityInterface)UnityInterfaceInterfaceManager.InstanceUnintyInterface;
		var v = GetComponent<MobileEntityGameObject> ();
		v.Uuid = UUI.newPlayer();
	}
	
	// Update is called once per frame
	void Update () {
		
	}	
}
