using System.Timers;
using System.Collections.Generic;
using System;
using System.Diagnostics;
namespace Galaxpeer
{
	public delegate void TickHandler (long time);

	public class PsycicManager{
		public static event TickHandler OnTick;

		List<MobileEntity> objects = new List<MobileEntity>();
		private static PsycicManager instance;
 		private static object syncRoot = new Object();

		private List<MobileEntity> created = new List<MobileEntity>();
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
 	  						instance.setTicks();
 					}
				}

				return instance;
			}
		}

		public void AddEntity(MobileEntity entity)
		{
			created.Add (entity);
		}

		private Timer timer1;
		public void setTicks(){
			Debug.Print("ah");
			System.Timers.Timer aTimer = new System.Timers.Timer();
			aTimer.Elapsed+=new ElapsedEventHandler(Tick);
			aTimer.Interval=100;
			aTimer.Enabled=true;
		}

	
		public void RemoveEntity(MobileEntity entity)
		{
			destroyed.Add (entity);
		}

		public static void Tick(object source,ElapsedEventArgs e)
		{
			var pm = PsycicManager.Instance;
			while (pm.created.Count != 0) {
				var entity = pm.created [0];
				pm.objects.Add (entity);
				EntityManager.Entities [entity.Uuid] = entity;
				UnityInterfaceInterfaceManager.InstanceUnintyInterface.SpawnModel (entity);
				pm.created.Remove (entity);
			}

			foreach (MobileEntity moe in pm.objects) {
				moe.LocationUpdate (0.02);
				if (moe.OwnedBy == LocalPlayer.Instance.Uuid) {
					foreach (MobileEntity moe2 in pm.objects) {
						if (moe.CheckCollision (moe2)) {
							moe.Collide (moe2);
						}
					}
				}
			}
			pm.Cleanup ();
			if (OnTick != null) {
				OnTick (DateTime.UtcNow.Ticks);
			}
		}

		private void Cleanup()
		{
			while (destroyed.Count != 0) {
				var entity = destroyed [0];
				objects.Remove (entity);
				EntityManager.Remove (entity);
				UnityInterfaceInterfaceManager.InstanceUnintyInterface.Destroy (entity);
				destroyed.Remove (entity);
			}
		}
	}
}