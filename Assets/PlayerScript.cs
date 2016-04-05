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

	System.Random rnd = new System.Random();
	long lastTick=0;
	public void Update(){
		UnityUnityInterface UUI = (UnityUnityInterface)UnityInterfaceInterfaceManager.InstanceUnintyInterface;

		if (Input.GetKeyDown(KeyCode.Space)) {
			UUI.shootplayer();
		}
		if (Input.GetKey (KeyCode.W)) {
			UUI.accaleratePlayer(1);
		} else {
			if(Input.GetKey(KeyCode.S)){
				UUI.accaleratePlayer(-1);
			}
		}
		Galaxpeer.Vector3 uprightspin = new Galaxpeer.Vector3 (0, 0, 0);
		if (Input.GetKey (KeyCode.UpArrow)) {
			uprightspin.X=uprightspin.X+.1f;
		}
		if (Input.GetKey (KeyCode.DownArrow)) {
			uprightspin.X=uprightspin.X-.1f;
		}
		if (Input.GetKey (KeyCode.LeftArrow)) {
			uprightspin.Y=uprightspin.Y+.1f;
		}
		if (Input.GetKey (KeyCode.RightArrow)) {
			uprightspin.Y=uprightspin.Y-.1f;
		}
		UUI.rotateplayer (uprightspin);
	}
}
