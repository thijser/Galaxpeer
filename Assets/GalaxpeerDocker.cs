using UnityEngine;
using Galaxpeer;
using System;
using System.Collections;
using System.Net;
using System.Collections.Generic;
using System.Threading;

public class GalaxpeerDocker : MonoBehaviour {
	public GameObject baseOtherPlayer;
	public GameObject baseAsteroid;
	public GameObject baseRocket;
	public string targetIP;
	public string targetPort;
	Dictionary<Guid, GameObject> gameObjects = new Dictionary<Guid, GameObject> ();

	void Start () {
		var UDPC = new UDPConnectionManager ();
			Game.Init (UDPC);
			print(UDPC.portstore);

		if (targetIP != null) {
			Game.ConnectionManager.Connect(new ConnectionMessage(LocalPlayer.Instance.Uuid,IPAddress.Parse(targetIP),int.Parse(targetPort),LocalPlayer.Instance.Location));
		}
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
			GameObject toInstantiate = null;

			switch(spawn.Type){
			case MobileEntity.EntityType.Player:
				toInstantiate = baseOtherPlayer;
				break;
			case MobileEntity.EntityType.Rocket:
				toInstantiate = baseRocket;
				break;
			case MobileEntity.EntityType.Asteroid:
				toInstantiate = baseAsteroid;
				break;
			}

			if (toInstantiate != null) {
				GameObject obj = (GameObject)Instantiate (toInstantiate, Conversion.ToUnity (spawn.Location), Conversion.ToUnity (spawn.Rotation));
				gameObjects [spawn.Uuid] = obj;
				obj.GetComponent<MobileEntityGameObject> ().Uuid = spawn.Uuid;
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
