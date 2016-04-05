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

	long lastTick=0;
	public void Update(){
		UnityUnityInterface UUI = (UnityUnityInterface)UnityInterfaceInterfaceManager.InstanceUnintyInterface;

		if (Input.GetKeyDown ("space")) {
			UUI.shootplayer();
		}
		if (Input.GetKeyDown (KeyCode.W)) {
			UUI.accaleratePlayer(0.01);
		} else {
			if(Input.GetKeyDown(KeyCode.S)){
				UUI.accaleratePlayer(0.01);
			}
		}
		Galaxpeer.Vector3 uprightspin = new Galaxpeer.Vector3 (0, 0, 0);
		if (Input.GetKeyDown (KeyCode.UpArrow)) {
			uprightspin.X=uprightspin.X+1;
		}
		if (Input.GetKeyDown (KeyCode.DownArrow)) {
			uprightspin.X=uprightspin.X-1;
		}
		if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			uprightspin.Y=uprightspin.Y+1;
		}
		if (Input.GetKeyDown (KeyCode.RightArrow)) {
			uprightspin.Y=uprightspin.Y-1;
		}

	}

}
