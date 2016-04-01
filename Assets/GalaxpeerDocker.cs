using UnityEngine;
using Galaxpeer;
using System;
using System.Collections;
using System.Collections.Generic;

public class GalaxpeerDocker : MonoBehaviour {
	public GameObject baseOtherPlayer;
	public GameObject baseAsstroid;
	public GameObject baseRocket; 


	
	// Update is called once per frame
	void Update () {
		handleSpawns ();
	}
	
	public void handleSpawns(){
		UnityUnityInterface UUI = (UnityUnityInterface)UnityInterfaceInterfaceManager.InstanceUnintyInterface;
		return;
		List<MobileEntity> toSpawn = UUI.getSpawns ();
		foreach (MobileEntity spawn in toSpawn) {
			switch(spawn.Type){
				case MobileEntity.EntityType.Player:break;
				case MobileEntity.EntityType.Rocket:break;
				case MobileEntity.EntityType.Asteroid:break;
			}
		}
	}
}
