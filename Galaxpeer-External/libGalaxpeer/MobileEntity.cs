using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace Galaxpeer
{
	public delegate void EntityUpdateHandler(MobileEntity entity, bool owned);

	public abstract class MobileEntity
	{
		const long UPDATE_PRETIMEOUT = 1500 * TimeSpan.TicksPerMillisecond;
		const long UPDATE_TIMEOUT = 3500 * TimeSpan.TicksPerMillisecond;
		const long PERIODIC_UPDATE_INTERVAL = 1000 * TimeSpan.TicksPerMillisecond;
		const int LIVENESS_TICK_INTERVAL = 1000;
		const int HANDOVER_TIMEOUT = 1500;
		const int HANDOVER_COOLDOWN = 1000;

		public static event EntityUpdateHandler OnLocationUpdate;
		public static event EntityUpdateHandler OnPeriodicUpdate;
		public static event EntityUpdateHandler BeforeTimeout;
		public static event EntityUpdateHandler OnTimeout;
		public static event EntityUpdateHandler OnDestroy;

		protected static Random rnd = new Random ();

		public enum EntityType : byte { Player = 1, Rocket = 2, Asteroid = 3 };

		protected Vector3 location;
		protected Quaternion rotation;
		protected Vector3 velocity;

		public Vector3 Location
		{
			get {
				return location;
			}
			set {
				location = value;
				fireUpdate (IsMine);
			}
		}

		public Quaternion Rotation
		{
			get {
				return rotation;
			}
			set {
				rotation = value;
				fireUpdate (IsMine);
			}
		}

		public Vector3 Velocity
		{
			get {
				return velocity;
			}
			set {
				velocity = value;
				fireUpdate (IsMine);
			}
		}


		protected abstract int MaxHealth { get; }
		public int Health;

		private void createLivenessTimer ()
		{
			livenessTimer = new Timer (this.onLivenessTick, null, LIVENESS_TICK_INTERVAL, LIVENESS_TICK_INTERVAL);
		}

		private void onLivenessTick (object _)
		{
			if (IsMine) {
				if (OnPeriodicUpdate != null) {
					OnPeriodicUpdate (this, IsMine);
				}
			} else {
				long age = Age;
				if (age >= UPDATE_TIMEOUT) {
					if (OnTimeout != null) {
						OnTimeout (this, IsMine);
					}
				} else if (age >= UPDATE_PRETIMEOUT) {
					if (BeforeTimeout != null) {
						BeforeTimeout (this, IsMine);
					}
				}
			}
		}
		/*
		private void setUpdateTimer ()
		{
			lock (ownershipLock) {
				try {
					livenessTimer.Dispose ();
				} catch (Exception) {}
				livenessTimer = new Timer (this.sendPeriodicUpdate, null, PERIODIC_UPDATE_INTERVAL, PERIODIC_UPDATE_INTERVAL);
			}
		}

		private void setBeforeTimeoutTimer ()
		{
			lock (ownershipLock) {
				try {
					livenessTimer.Dispose ();
				} catch (Exception) {}
				livenessTimer = new Timer (this.beforeTimeout, null, UPDATE_PRETIMEOUT, UPDATE_TIMEOUT);
			}
		}

		private void setTimeoutTimer ()
		{
			lock (ownershipLock) {
				try {
					livenessTimer.Dispose();
				} catch (Exception) {}
				livenessTimer = new Timer (this.onUpdateTimeout, null, UPDATE_TIMEOUT - UPDATE_PRETIMEOUT, UPDATE_TIMEOUT);
			}
		}
		*/
		private bool isMine = false;
		public bool IsMine {
			get {
				return isMine;
			}
			set {
				//lock (ownershipLock) {
				//	if (value && !isMine) {
				//		setUpdateTimer ();
				//	} else if (!value && isMine) {
				//		setBeforeTimeoutTimer ();
				//	}
					isMine = value;
				//}
			}
		}

		/*void sendPeriodicUpdate (object _) {
			if (OnPeriodicUpdate != null) {
				OnPeriodicUpdate (this, IsMine);
			}
		}

		void beforeTimeout (object _) {
			if (BeforeTimeout != null) {
				BeforeTimeout (this, IsMine);
			}
			setTimeoutTimer ();
		}

		void onUpdateTimeout (object _) {
			if (OnTimeout != null) {
				OnTimeout (this, IsMine);
			}
		}*/

		public abstract EntityType Type { get; }
		public float Size;
		public Guid Uuid;

		private object ownershipLock = new object ();
		private Guid ownedBy;
		public Guid OwnedBy {
			get {
				return ownedBy;
			}
			set {
				lock (ownershipLock) {
					IsMine = value.Equals (LocalPlayer.LocalUuid);
					ownedBy = value;
				}
			}
		}

		protected Timer handoverTimer;
		protected Timer livenessTimer;
		protected volatile bool isHandingOver = false;
		public long LastUpdate = 0;

		public long Age {
			get {
				return DateTime.UtcNow.Ticks - LastUpdate;
			}
		}

		public MobileEntity ()
		{
			LastUpdate = DateTime.UtcNow.Ticks;
			createLivenessTimer();
			Uuid = Guid.NewGuid ();
			location = new Vector3 (0, 0, 0);
			rotation = new Quaternion (0, 0, 0, 1);
			velocity = new Vector3 (0, 0, 0);
			Size = .8f;
			Health = MaxHealth;

			//lock (ownershipLock) {
				OwnedBy = LocalPlayer.LocalUuid;
			//	if (!IsMine) {
			//		setBeforeTimeoutTimer ();
			//	}
			//}
		}

		public MobileEntity(IFullLocationMessage message)
		{
			createLivenessTimer();
			Uuid = message.Uuid;
			copyMessageData (message);
			Size = .8f;
			Health = MaxHealth;
			fireUpdate (false);

			//lock (ownershipLock) {
				OwnedBy = message.OwnedBy;
			//	if (!IsMine) {
			//		setBeforeTimeoutTimer ();
			//	}
			//}
		}

		private void copyMessageData(IFullLocationMessage message)
		{
			location = message.Location;
			rotation = message.Rotation;
			velocity = message.Velocity;

			Health = message.Health;
			OwnedBy = message.OwnedBy;
			LastUpdate = message.Timestamp;
			livenessTimer.Change (LIVENESS_TICK_INTERVAL, LIVENESS_TICK_INTERVAL);
		}

		public void Update(IFullLocationMessage message)
		{
			// Ownership conflict! Client with lowest UUID takes ownership
			lock (ownershipLock) {
				if (IsMine && !isHandingOver && OwnedBy.CompareTo (message.OwnedBy) < 0) {
					if (Game.Config.PrintEntities) {
						Console.WriteLine ("Ownership conflict over {0} between me ({1}) and {2}", message.Uuid, OwnedBy, message.OwnedBy);
					}
					Takeover (message.SourceClient);
				} else if (IsMine || message.Timestamp > LastUpdate) {
					copyMessageData (message);
					//setBeforeTimeoutTimer ();
					fireUpdate (false);
				}
			}
		}

		protected void TryHandover ()
		{
			Client closest;
			if (Position.ClosestClient (location, out closest)) {
				Handover (closest);
			} else {
				// TODO: What now?
			}
		}

		protected void Handover (Client client)
		{
			lock (ownershipLock) {
				if (IsMine) {
					isHandingOver = true;
					client.Connection.Send (new HandoverMessage (this));
					handoverTimer = new Timer (this.onHandoverTimeout, this, HANDOVER_TIMEOUT, HANDOVER_TIMEOUT);
				}
			}
		}

		public void Takeover (Client client)
		{
			lock (ownershipLock) {
				client.Connection.Send (new TakeoverMessage (this));
				OnTakeover (LocalPlayer.Instance.Uuid);
			}
		}

		public void Takeover ()
		{
			lock (ownershipLock) {
				Game.ConnectionManager.SendInRoi (new TakeoverMessage (this), Location);
				OnTakeover (LocalPlayer.Instance.Uuid);
			}
		}

		public void OnTakeover (Guid uuid)
		{
			lock (ownershipLock) {
				isHandingOver = true;
				OwnedBy = uuid;
				//try {
				if (handoverTimer != null) {
					handoverTimer.Dispose ();
				}
				//} catch (Exception) {}
				handoverTimer = new Timer (onHandoverCooldown, null, HANDOVER_COOLDOWN, Timeout.Infinite);
			}
			//Game.ConnectionManager.SendInRoi (new LocationMessage (this), Location);
			livenessTimer.Change (0, LIVENESS_TICK_INTERVAL);
		}

		void onHandoverCooldown (object obj)
		{
			if (handoverTimer != null) {
				handoverTimer.Dispose ();
				handoverTimer = null;
			}
			isHandingOver = false;
		}

		protected void onHandoverTimeout (object obj)
		{
			lock (ownershipLock) {
				if (handoverTimer != null) {
					handoverTimer.Dispose ();
					handoverTimer = null;
				}
				TryHandover ();
			}
		}
			
		protected void fireUpdate(bool owned)
		{
			LastUpdate = DateTime.UtcNow.Ticks;
			if (OnLocationUpdate != null) {
				OnLocationUpdate (this, owned);
			}
		}

		public virtual void Collide (MobileEntity other)
		{
			float difX = other.Velocity.X - Velocity.X;
			float difY = other.Velocity.X - Velocity.X;
			float difZ = other.Velocity.X - Velocity.X;
			int difHealth = (int) Math.Round (difX * difX + difY * difY + difZ * difZ);
			Vector3 myVelocity = Velocity;

			Health -= difHealth;
			other.Health -= difHealth;
			if (Health <= 0) {
				this.Destroy ();
			} else {
				Velocity = other.Velocity;
			}

			if (other.Health <= 0) {
				other.Destroy ();
			} else {
				other.Velocity = myVelocity;
			}
		}

		public virtual void Destroy ()
		{
			PsycicManager.Instance.RemoveEntity (this);
			lock (ownershipLock) {
				if (handoverTimer != null) {
				//try {
					handoverTimer.Dispose ();
				//} catch (Exception) {}
				}
				if (livenessTimer != null) {
				//try {
					livenessTimer.Dispose ();
				//} catch (Exception) {}
				}
			}

			if (OnDestroy != null) {
				OnDestroy (this, IsMine);
			}
		}

		// If entity is mine and new position is closer to other player, hand over to that player
		// If entity is mine and outside my ROI, but not closer to other player, destroy
		// If entity is not mine and outside my ROI, destroy
		public void LocationUpdate (double stepsize)
		{
			if (Health > 0) {
				float nx = (float)(Location.X + stepsize * Velocity.X); 
				float ny = (float)(Location.Y + stepsize * Velocity.Y);
				float nz = (float)(Location.Z + stepsize * Velocity.Z);
				location = new Vector3 (nx, ny, nz);

				lock (ownershipLock) {
					if (IsMine) {
						if (!isHandingOver) {
							Client closest;
							if (Position.ClosestClient (location, out closest)) {
								Handover (closest);
							} else if (!Position.IsEntityInRoi (LocalPlayer.Instance.Location, location)) {
								Destroy ();
							}
						}
					} else {
						if (!Position.IsEntityInRoi (LocalPlayer.Instance.Location, Location)) {
							Destroy ();
						}
					}
				}
			}
		}

		public void AccelerateForward (float stepsize, float acceleration, float maxspeed)
		{
			if (Health > 0) {
				Vector3 f = Rotation.GetForwardVector ();
				Vector3 v = velocity + (stepsize * (acceleration * f));

				if ((v.X * v.X) + (v.Y * v.Y) + (v.Z * v.Z) >= maxspeed * maxspeed) {
					v *= 0.8f;
				}
				Velocity = v;
			}
		}

		public void Rotate (double up, double right, double spin)
		{
			if (Health > 0) {
				Quaternion r = Quaternion.CreateFromYawPitchRoll ((float)right, (float)up, (float)spin);
				Rotation *= r;
			}
		}

		public bool CheckCollision (MobileEntity other)
		{
			if (other == null) {
				return false;
			}
			if (other.Equals (this)) {
				return false;
			}
			if (this.Health <= 0 || other.Health <= 0) {
				return false;
			}
			double Xdist = Location.X - other.Location.X;
			double Ydist = Location.Y - other.Location.Y;
			double Zdist = Location.Z - other.Location.Z;
			if (Size + other.Size > Math.Sqrt (Xdist * Xdist + Ydist * Ydist + Zdist * Zdist)) {
				return true;
			}
			return false;
		}
	}

	public class Player: MobileEntity
	{
		public override EntityType Type {
			get {
				return EntityType.Player;
			}
		}

		protected override int MaxHealth {
			get {
				return 100;
			}
		}

		public Player() {}

		public Player(ConnectionMessage message)
		{
			Uuid = message.Uuid;
			OwnedBy = Uuid;
			Location = message.Location;
		}

		public Player(IFullLocationMessage message) : base(message) {}
	}

	public class Rocket : MobileEntity
	{
		public override EntityType Type {
			get {
				return EntityType.Rocket;
			}
		}

		protected override int MaxHealth {
			get {
				return 10;
			}
		}

		const float VELOCITY = 50;

		public Rocket(Vector3 location, Quaternion rotation)
		{
			Vector3 v = rotation.GetForwardVector ();
		
			this.location = location + (v * (Size + LocalPlayer.Instance.Size + .5f));
			this.rotation = rotation;

			float Vx = (float)(VELOCITY * v.X);
			float Vy = (float)(VELOCITY * v.Y);
			float Vz = (float)(VELOCITY * v.Z);
			Velocity = new Vector3 (Vx, Vy, Vz);
		}

		public Rocket(IFullLocationMessage message) : base(message) {}

		public override void Collide (MobileEntity other)
		{
			//float difX = other.Velocity.X - Velocity.X;
			//float difY = other.Velocity.X - Velocity.X;
			//float difZ = other.Velocity.X - Velocity.X;
			//this.Velocity = other.Velocity;
			//Health = (int)(Health - (difX * difX + difY * difY + difZ * difZ));
			other.Health -= 20;
			this.Destroy ();
		}
	}

	public class Asteroid : MobileEntity
	{
		public override EntityType Type {
			get {
				return EntityType.Asteroid;
			}
		}

		private const int MinHealth = 20;
		protected override int MaxHealth {
			get {
				return 200;
			}
		}
		public Asteroid()
		{
			location = Position.Near (LocalPlayer.Instance.Location);
			velocity = new Vector3 ((float) rnd.NextDouble() -.5f, (float) rnd.NextDouble() -.5f, (float) rnd.NextDouble() -.5f);
			Rotate (rnd.NextDouble (), rnd.NextDouble(), rnd.NextDouble());
			Health = rnd.Next (MinHealth, MaxHealth);
		}

		public Asteroid(IFullLocationMessage message) : base(message) {}
	}

		

	public class LocalPlayer : Player
	{
		const long FIRE_INTERVAL = TimeSpan.TicksPerSecond * 1;

		public bool IsSpawning = false;
		private Timer spawnTimer;

		public override EntityType Type {
			get {
				return EntityType.Player;
			}
		}

		public static Guid LocalUuid {
			get {
				if (instance != null) {
					return instance.Uuid;
				}
				return Guid.Empty;
			}
		}

		public long LastShotFired = 0;

		private static volatile LocalPlayer instance;
		private static object syncRoot = new Object ();

		private LocalPlayer ()
		{
			Health = 0;
			Spawn ();
		}

		private float rndLoc ()
		{
			return (float)rnd.Next (int.MinValue / 1000000, int.MaxValue / 1000000);
		}

		public void Spawn ()
		{
			if (!IsSpawning) {
				IsSpawning = true;
				Velocity = new Vector3 (0, 0, 0);
				Location = new Vector3 (rndLoc(), rndLoc(), rndLoc());
				spawnTimer = new Timer (selectSpawnPoint, null, 5000, Timeout.Infinite);
			}
		}

		public override void Destroy ()
		{
			Spawn ();
		}

		private void selectSpawnPoint (object _)
		{
			Client closest = Position.ClosestClient ();
			if (closest == null) {
				Location = new Vector3 (rnd.Next (0, 200), rnd.Next (0, 200), rnd.Next (0, 200));
			} else {
				Location = Position.Near (closest.Player.Location, 10);//Position.ROI_RADIUS / 2);
			}
			spawnTimer.Dispose ();
			spawnTimer = new Timer (completeSpawn, null, 1000, Timeout.Infinite);
		}

		private void completeSpawn (object _)
		{
			spawnTimer.Dispose ();
			spawnTimer = null;
			Health = MaxHealth;
			IsSpawning = false;
		}

		public Rocket Fire ()
		{
			if (DateTime.UtcNow.Ticks - LastShotFired >= FIRE_INTERVAL) {
				LastShotFired = DateTime.UtcNow.Ticks;
				return new Rocket (this.Location, this.Rotation);
			}
			return null;
		}


		public static LocalPlayer Instance {
			get {
				if (instance == null) {
					lock (syncRoot) {
						if (instance == null) {
							instance = new LocalPlayer ();
							instance.OwnedBy = instance.Uuid;
							PsycicManager.Instance.AddEntity (instance);
						}
					}
				}

				return instance;
			}
		}
	}
}
