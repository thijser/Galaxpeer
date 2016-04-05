using System.Collections.Generic;
using System.Timers;

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
					}
				}

				return instance;
			}
		}

		public void AddEntity(MobileEntity entity)
		{
			created.Add (entity);
		}

<<<<<<< HEAD
		private Timer timer1;
		public void setTicks(){
			timer1 = new Timer();
			timer1.Tick += new EventHandler(tick);
			timer1.Interval = 100; // in miliseconds
			timer1.Start();

		}
		public void tick(){
			
=======
		public void RemoveEntity(MobileEntity entity)
		{
			destroyed.Add (entity);
		}

		public void Tick()
		{
			long time = DateTime.UtcNow.Ticks;

			while (created.Count != 0) {
				var entity = created [0];
				objects.Add (entity);
				EntityManager.Entities [entity.Uuid] = entity;
				UnityInterfaceInterfaceManager.InstanceUnintyInterface.SpawnModel (entity);
				created.Remove (entity);
			}

>>>>>>> 72bf5dfdc8c400bd7d4e159df1665d136d877c5c
			foreach (MobileEntity moe in objects) {
				moe.LocationUpdate (0.02);
				if (moe.OwnedBy == LocalPlayer.Instance.Uuid) {
					foreach (MobileEntity moe2 in objects) {
						if (moe.CheckCollision (moe2)) {
							moe.Collide (moe2);
						}
					}
				}
			}

			if (OnTick != null) {
				OnTick (time);
			}

			Cleanup ();
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