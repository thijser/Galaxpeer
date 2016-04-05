using System;
using System.Collections;
using System.Collections.Generic;

namespace Galaxpeer
{
	public class UnityUnityInterface : UnityInterfaceInterface
	{
		//Dictionary<Guid,MobileEntity> mapping;
		List<MobileEntity> ToSpawn;
		List<MobileEntity> ToDestroy;

		public UnityUnityInterface()
		{
			ToSpawn = new List<MobileEntity> ();
			ToDestroy = new List<MobileEntity> ();
			//mapping = new Dictionary<Guid,MobileEntity> ();

		}
		public void SpawnModel(MobileEntity baseEntity){
			ToSpawn.Add (baseEntity);
			//mapping.Add(baseEntity.Uuid,baseEntity);
		}

		public void Destroy(MobileEntity baseEntity){
			// TODO: Remove from EntityManager mapping
			ToSpawn.Remove (baseEntity);
			ToDestroy.Add (baseEntity);
		}

		public MobileEntity GetEntity(Guid guid)
		{
			return EntityManager.Get (guid);
		}

		public List<MobileEntity> getSpawns(){
			lock (ToSpawn){
				var ret = ToSpawn;
				ToSpawn = new List<MobileEntity> ();
				return ret;
			}
		}

		public List<MobileEntity> getDestroys() {
			lock (ToDestroy) {
				var ret = ToDestroy;
				ToDestroy = new List<MobileEntity> ();
				return ret;
			}
		}

		public Guid newPlayer(){
			LocalPlayer.Instance.Spawn ();
			return LocalPlayer.Instance.Uuid;
		}
		public void shootplayer(){
			PsycicManager.Instance.AddEntity(LocalPlayer.Instance.Fire());
		}

		public void rotateplayer(Vector3 uprightspin){
			LocalPlayer.Instance.Rotate(uprightspin.X,uprightspin.Y,uprightspin.Z);
		}

		public void accaleratePlayer(float acc){
			LocalPlayer.Instance.AccelerateForward (1f, acc, 5f);
		}

	}

}



