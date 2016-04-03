using System.Collections.Generic;
using System;

namespace Galaxpeer
{
	public delegate void TickHandler (long time);

	public class PsycicManager{
		public static event TickHandler OnTick;

		List<MobileEntity> objects = new List<MobileEntity>();
		private static PsycicManager instance;
 		private static object syncRoot = new Object();
		private List<MobileEntity> destroyed = new List<MobileEntity>();
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

		public void AddEntity(MobileEntity entity)
		{
			objects.Add (entity);
			UnityInterfaceInterfaceManager.InstanceUnintyInterface.SpawnModel (entity);
		}

		public void RemoveEntity(MobileEntity entity)
		{
			destroyed.Add (entity);
		}

		public void Tick()
		{
			long time = DateTime.UtcNow.Ticks;

			if (OnTick != null) {
				OnTick (time);
			}

			foreach (MobileEntity moe in objects) {
				moe.LocationUpdate (1);
				if (moe.OwnedBy == LocalPlayer.Instance.Uuid) {
					foreach (MobileEntity moe2 in objects) {
						if (moe.CheckCollision (moe2)) {
							moe.Collide (moe2);
						}
					}
				}
			}

			Cleanup ();
		}

		private void Cleanup()
		{
			while (destroyed.Count != 0) {
				var entity = destroyed [0];
				objects.Remove (entity);
				destroyed.Remove (entity);
			}
		}
	}
}