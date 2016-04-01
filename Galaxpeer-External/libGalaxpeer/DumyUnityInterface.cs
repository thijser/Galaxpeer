using System;

namespace Galaxpeer
{
	public class DumyUnityInterface : UnityInterfaceInterface
	{
		public void SpawnModel(MobileEntity baseEntity){}
		public void Destroy(MobileEntity baseEntity){}
		public void Move(MobileEntity baseEntity){}
		public MobileEntity GetEntity(Guid guid){ return null;}
		public DumyUnityInterface ()
		{
		}
	}
}

