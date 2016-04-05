using UnityEngine;
using System;
using System.Collections;
using Galaxpeer;

public class PlayerScript : MonoBehaviour {

	private Timer timer1; 
	// Use this for initialization
	void Start () {
		UnityUnityInterface UUI = (UnityUnityInterface)UnityInterfaceInterfaceManager.InstanceUnintyInterface;
		var v = GetComponent<MobileEntityGameObject> ();
		v.Uuid = UUI.newPlayer();

		timer1 = new Timer();
		timer1.Tick += new EventHandler(tick);
		timer1.Interval = 100; // in miliseconds
		timer1.Start();

	}

	long lastTick=0;

	void tick(){
		UnityUnityInterface UUI = (UnityUnityInterface)UnityInterfaceInterfaceManager.InstanceUnintyInterface;
		UUI.tick ();
	}
}
