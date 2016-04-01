using System.Collections.Generic;
using System;

namespace Galaxpeer
{
	public class PsycicManager{
		
		List<MobileEntity> objects;
		private static PsycicManager instance;
 		private static object syncRoot = new Object();
		public List<MobileEntity> Destoyed;
		public static PsycicManager Instance
		{
			get 
			{
				if (instance == null) 
				{
					lock (syncRoot) 
					{
						if (instance == null) 
							instance = new PsycicManager();
					}
				}

				return instance;
			}
		}
		public void addEntity(MobileEntity entity){
			objects.Add (entity);
			UnityInterfaceInterfaceManager.InstanceUnintyInterface.SpawnModel (entity);
		}

		public void tick(){
			
			foreach (MobileEntity moe in objects) {
				moe.LocationUpdate (1);
				if (moe.ownedBy == LocalPlayer.Instance.Uuid) {
					foreach (MobileEntity moe2 in objects) {
						if (moe.CheckCollision (moe2)) {
							moe.collide (moe2);
						}	
					}
				}
			}

			Cleanup ();
		}

		private void Cleanup(){
			while (Destoyed.Count != 0) {
				var entity = Destoyed [0];
				objects.Remove (entity);
				Destoyed.Remove (entity);
			}
		}
	}
}