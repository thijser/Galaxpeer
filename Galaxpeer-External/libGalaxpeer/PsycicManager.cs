
using System.Collections.Generic;
using System;
using System.Threading;

namespace Galaxpeer
{
	public delegate void TickHandler (long time);

	public class PsycicManager{
		public static event TickHandler OnTick;

		List<MobileEntity> objects = new List<MobileEntity>();
		private static PsycicManager instance;

		private static object tickLock = new object ();
 		private static object syncRoot = new object ();

		private List<MobileEntity> created = new List<MobileEntity>();
		private List<MobileEntity> destroyed = new List<MobileEntity>();

		private Timer timer;


		public static PsycicManager Instance
		{
			get 
			{
				if (instance == null) 
				{
					lock (syncRoot)
					{
						if (instance == null) {
							instance = new PsycicManager ();
						}
 					}
				}

				return instance;
			}
		}

		public void Init()
		{
			timer = new Timer (Tick, null, 0, 100);
		}

		public void AddEntity(MobileEntity entity)
		{
			if (entity != null) {
				EntityManager.Add(entity.Uuid, entity);
				created.Add (entity);
			}
		}
	
		public void RemoveEntity(MobileEntity entity)
		{
			EntityManager.Remove (entity);
			destroyed.Add (entity);
		}

		private static bool moved = false;
		private static Random rnd = new Random ();
		private static Vector3 previousLocation;
		private static Timer neighbourTimer = new Timer ((object _) => { Game.Measure.LogNeighbours(); }, null, Timeout.Infinite, Timeout.Infinite);
		public static void Tick(object _)
		{
			Game.Measure.BeginPhysics ();
			lock (tickLock) {

				DateTime time = DateTime.UtcNow;

				if (Game.Config.MeasureFail) {
					if (time.Minute % 2 == 0 && !moved) {
						moved = true;
						previousLocation = LocalPlayer.Instance.Location;
						LocalPlayer.Instance.Location = new Vector3 (rnd.Next (0, 150), rnd.Next (0, 150), rnd.Next (0, 150));
						Console.WriteLine ("Jumped to {0}", LocalPlayer.Instance.Location);
						neighbourTimer.Change (30000, Timeout.Infinite);
					} else if (time.Minute % 2 == 1 && moved) {
						moved = false;
						LocalPlayer.Instance.Location = previousLocation;
						Console.WriteLine ("Jumped to {0}", LocalPlayer.Instance.Location);
						neighbourTimer.Change (30000, Timeout.Infinite);
					}
				}

				var pm = PsycicManager.Instance;
				while (pm.created.Count != 0) {
					var entity = pm.created [0];
					pm.objects.Add (entity);
					if (entity != LocalPlayer.Instance) {
						UnityInterfaceInterfaceManager.InstanceUnintyInterface.SpawnModel (entity);
					}
					pm.created.Remove (entity);
				}

				foreach (MobileEntity moe in pm.objects) {
					moe.LocationUpdate (0.1);
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
					OnTick (time.Ticks);
				}
			}
			Game.Measure.EndPhysics ();
		}

		private void Cleanup()
		{
			while (destroyed.Count != 0) {
				var entity = destroyed [0];
				objects.Remove (entity);
				UnityInterfaceInterfaceManager.InstanceUnintyInterface.Destroy (entity);
				destroyed.Remove (entity);
			}
		}
	}
}