using System.Collections.Generic;
using System;

namespace Galaxpeer
{
	public delegate void TickHandler ();

	public class PsycicManager{
		public static event TickHandler OnTick;

		List<MobileEntity> objects = new List<MobileEntity>();
		private static PsycicManager instance;
 		private static object syncRoot = new Object();
		public List<MobileEntity> Destoyed = new List<MobileEntity>();
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
			if (OnTick != null) {
				OnTick ();
			}

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