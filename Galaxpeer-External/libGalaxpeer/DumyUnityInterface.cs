using System;
using System.Collections.Generic;
namespace Galaxpeer
{
	public class DumyUnityInterface : UnityInterfaceInterface
	{
		public void SpawnModel(MobileEntity baseEntity){}
		public void Destroy(MobileEntity baseEntity){}
		public void Move(MobileEntity baseEntity){}
		public MobileEntity GetEntity(Guid guid){ return null;}
		public List<MobileEntity> getSpawns(){
			return new List<MobileEntity> ();}
		public DumyUnityInterface ()
		{
		}
	}
}

