using System;
using System.Collections.Generic;
namespace Galaxpeer
{
	public interface UnityInterfaceInterface
	{
		void SpawnModel(MobileEntity baseEntity);
		void Destroy(MobileEntity baseEntity);
		void Move(MobileEntity baseEntity);
	 	MobileEntity GetEntity(Guid guid);
		List<MobileEntity> getSpawns();
	}
}

