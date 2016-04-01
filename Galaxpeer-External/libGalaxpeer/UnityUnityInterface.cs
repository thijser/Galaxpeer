using System;
using System.Collections;
using System.Collections.Generic;

namespace Galaxpeer
{
	public class UnityUnityInterface : UnityInterfaceInterface
	{
		//Dictionary<Guid,MobileEntity> mapping;
		List<MobileEntity> ToSpawn;

		public UnityUnityInterface()
		{
			ToSpawn = new List<MobileEntity> ();
			//mapping = new Dictionary<Guid,MobileEntity> ();

		}
		public void SpawnModel(MobileEntity baseEntity){
			ToSpawn.Add (baseEntity);
			//mapping.Add(baseEntity.Uuid,baseEntity);
		}

		public void Destroy(MobileEntity baseEntity){
			// TODO: Remove from EntityManager mapping
			ToSpawn.Remove (baseEntity);
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

		public Guid newPlayer(){
			LocalPlayer.Instance.Spawn ();
			return LocalPlayer.Instance.Uuid;
		}
		public void shootplayer(){
		}
		public void rotateplayer(Vector3 uprightspin){
			LocalPlayer.Instance.rotate(uprightspin.X,uprightspin.Y,uprightspin.Z);
		}
		public void accaleratePlayer(double acc){
			
		}

	}

}



