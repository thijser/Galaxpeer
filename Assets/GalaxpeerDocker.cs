using UnityEngine;
using Galaxpeer;
using System;
using System.Collections;
using System.Collections.Generic;

public class GalaxpeerDocker : MonoBehaviour {
	public GameObject baseOtherPlayer;
	public GameObject baseAsteroid;
	public GameObject baseRocket;

	Dictionary<Guid, GameObject> gameObjects = new Dictionary<Guid, GameObject> ();

	void Start () {
		Game.Init (new UDPConnectionManager (36963));
		InvokeRepeating("Tick", 0, 0.02F);
	}

	
	// Update is called once per frame
	void Update () {
		handleSpawns ();
		handleDestroys ();
	}
	
	public void handleSpawns(){
		UnityUnityInterface UUI = (UnityUnityInterface)UnityInterfaceInterfaceManager.InstanceUnintyInterface;
		List<MobileEntity> toSpawn = UUI.getSpawns ();
		foreach (MobileEntity spawn in toSpawn) {
			switch(spawn.Type){
			case MobileEntity.EntityType.Player:break;
			case MobileEntity.EntityType.Rocket:break;
			case MobileEntity.EntityType.Asteroid:
				GameObject asteroid = (GameObject)Instantiate (baseAsteroid, Conversion.ToUnity (spawn.Location), Conversion.ToUnity (spawn.Rotation));
				gameObjects [spawn.Uuid] = asteroid;
				asteroid.GetComponent<MobileEntityGameObject> ().Uuid = spawn.Uuid;
				break;
			}
		}
	}

	void handleDestroys()
	{
		UnityUnityInterface UUI = (UnityUnityInterface)UnityInterfaceInterfaceManager.InstanceUnintyInterface;
		List<MobileEntity> toDestroy = UUI.getDestroys ();
		foreach (MobileEntity destroy in toDestroy) {
			GameObject obj;
			if (gameObjects.TryGetValue (destroy.Uuid, out obj)) {
				GameObject.Destroy (obj);
				gameObjects.Remove (destroy.Uuid);
			}
		}
	}
}
