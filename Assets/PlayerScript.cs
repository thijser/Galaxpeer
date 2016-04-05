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

	public void Update(){
		UnityUnityInterface UUI = (UnityUnityInterface)UnityInterfaceInterfaceManager.InstanceUnintyInterface;

		if (Input.GetKeyDown(KeyCode.Space)) {
			UUI.shootplayer();
		}
		if (Input.GetKey (KeyCode.W)) {
			UUI.accaleratePlayer(.1f);
		} else {
			if(Input.GetKey(KeyCode.S)){
				UUI.accaleratePlayer(-.1f);
			}
		}
		Galaxpeer.Vector3 uprightspin = new Galaxpeer.Vector3 (0, 0, 0);
		if (Input.GetKey (KeyCode.UpArrow)) {
			uprightspin.X=uprightspin.X+.02f;
		}
		if (Input.GetKey (KeyCode.DownArrow)) {
			uprightspin.X=uprightspin.X-.02f;
		}
		if (Input.GetKey (KeyCode.LeftArrow)) {
			uprightspin.Y=uprightspin.Y-.02f;
		}
		if (Input.GetKey (KeyCode.RightArrow)) {
			uprightspin.Y=uprightspin.Y+.02f;
		}
		UUI.rotateplayer (uprightspin);
	}
}
