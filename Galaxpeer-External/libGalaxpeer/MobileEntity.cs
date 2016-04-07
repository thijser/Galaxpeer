using System;
using System.Runtime.InteropServices;

namespace Galaxpeer
{
	public delegate void EntityUpdateHandler(MobileEntity entity, bool owned);

	public abstract class MobileEntity
	{
		public static event EntityUpdateHandler OnLocationUpdate;
		public static event EntityUpdateHandler OnDestroy;

		public enum EntityType : byte { Player = 1, Rocket = 2, Asteroid = 3 };

		protected Vector3 location;
		protected Quaternion rotation;
		protected Vector3 velocity;

		bool? isMine = null;
		bool IsMine
		{
			get {
				if (isMine == null) {
					isMine = OwnedBy.Equals (LocalPlayer.LocalUuid); 
				}
				return (bool) isMine;
			}
		}

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

		public abstract EntityType Type { get; }
		public float Size;
		public Guid Uuid;
		public Guid OwnedBy;
		public long LastUpdate = DateTime.UtcNow.Ticks;

		public MobileEntity ()
		{
			Uuid = Guid.NewGuid ();
			OwnedBy = LocalPlayer.LocalUuid;
			location = new Vector3 (0, 0, 0);
			rotation = new Quaternion (0, 0, 0, 1);
			velocity = new Vector3 (0, 0, 0);
			Size = 1;
			Console.WriteLine ("Generated MobileEntity {0} of type {1} at {2}.{3}.{4}", Uuid, this.Type, Location.X, Location.Y, Location.Z);
		}

		public MobileEntity(LocationMessage message)
		{
			Uuid = message.Uuid;
			OwnedBy = message.SourceClient.Uuid;
			copyMessageData (message);
			Size = 1;
			Console.WriteLine ("Created MobileEntity {0} of type {1} from LocationMessage", Uuid, this.Type);
			fireUpdate (false);
		}

		private void copyMessageData(LocationMessage message)
		{
			location = message.Location;
			rotation = message.Rotation;
			velocity = message.Velocity;

			LastUpdate = message.Timestamp;
		}

		public void Update(LocationMessage message)
		{
			if (message.Timestamp > LastUpdate) {
				copyMessageData (message);
				fireUpdate (false);
			}
		}
			
		protected void fireUpdate(bool owned)
		{
			LastUpdate = DateTime.UtcNow.Ticks;
			if (OnLocationUpdate != null) {
				OnLocationUpdate (this, owned);
			}
		}

		public abstract void Collide (MobileEntity other);

		public virtual void Destroy ()
		{
			PsycicManager.Instance.RemoveEntity (this);

			if (OnDestroy != null) {
				OnDestroy (this, IsMine);
			}
		}

		public int Health;

		public void LocationUpdate (double stepsize)
		{
			float nx = (float)(Location.X + stepsize * Velocity.X); 
			float ny = (float)(Location.Y + stepsize * Velocity.Y);
			float nz = (float)(Location.Z + stepsize * Velocity.Z);
			location = new Vector3 (nx, ny, nz);

			if (!Position.IsEntityInRoi (LocalPlayer.Instance.Location, Location)) {
				if (!IsMine || !Position.IsInAnyRoi (Location)) {
					Destroy ();
				}
			}
		}

		public void AccelerateForward (float stepsize, float acceleration, float maxspeed)
		{
			Vector3 f = Rotation.GetForwardVector ();
			Vector3 v = velocity + (stepsize * (acceleration * f));

			//float Vx = Velocity.X + (float)(stepsize * (acceleration * Rotation.GetForwardVector ().X));
			//float Vy = Velocity.Y + (float)(stepsize * (acceleration * Rotation.GetForwardVector ().Y));
			//float Vz = Velocity.Z + (float)(stepsize * (acceleration * Rotation.GetForwardVector ().Z));
			//velocity = new Vector3 (Vx, Vy, Vz);
			if ((v.X * v.X) + (v.Y * v.Y) + (v.Z * v.Z) >= maxspeed * maxspeed) {
				v *= 0.8f;
			}
			Velocity = v;
		}

		public void Rotate (double up, double right, double spin)
		{
			Quaternion r = Quaternion.CreateFromYawPitchRoll ((float) right, (float) up, (float) spin);
			Rotation *= r;

		}

		public bool CheckCollision (MobileEntity other)
		{
			if (other.Equals (this))
				return false;
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

		public Player() {}

		public Player(ConnectionMessage message)
		{
			Uuid = message.Uuid;
			OwnedBy = Uuid;
			Location = message.Location;
		}

		public Player(LocationMessage message) : base(message) {}

		public override void Collide (MobileEntity other)
		{
			this.Velocity = other.Velocity;
		}
	}

	public class Rocket : MobileEntity
	{
		public override EntityType Type {
			get {
				return EntityType.Rocket;
			}
		}

		const float VELOCITY = 50;

		public Rocket(Vector3 location, Quaternion rotation)
		{
			Vector3 v = rotation.GetForwardVector ();
		
			this.location = location + (v * 3);
			this.rotation = rotation;

			float Vx = (float)(20 * v.X);
			float Vy = (float)(20 * v.Y);
			float Vz = (float)(20 * v.Z);
			Velocity = new Vector3 (Vx, Vy, Vz);
		}

		public Rocket(LocationMessage message) : base(message) {}

		public override void Collide (MobileEntity other)
		{
			float difX = other.Velocity.X - Velocity.X;
			float difY = other.Velocity.X - Velocity.X;
			float difZ = other.Velocity.X - Velocity.X;
			this.Velocity = other.Velocity;
			Health = (int)(Health - (difX * difX + difY * difY + difZ * difZ));
			other.Health = other.Health - 10;
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
		
		static Random rnd = new Random ();
		public Asteroid()
		{
			double s = rnd.NextDouble () % (2 * Math.PI);
			double t = rnd.NextDouble () % (2 * Math.PI);

			double x = Math.Cos (s) * Math.Cos (t);
			double y = Math.Sin (s) * Math.Cos (t);
			double z = Math.Sin (t);

			float distance = 80; //Position.ROI_RADIUS / 2; //- 1;
			if (rnd.NextDouble () > .5) {
				distance *= -1;
			}

			location = LocalPlayer.Instance.Location + (new Vector3 ((float) x, (float) y, (float) z) * distance);
			velocity = new Vector3 ((float) rnd.NextDouble() -.5f, (float) rnd.NextDouble() -.5f, (float) rnd.NextDouble() -.5f);
			Rotate (rnd.NextDouble (), rnd.NextDouble(), rnd.NextDouble());
		}

		public Asteroid(LocationMessage message) : base(message) {}

		public override void Collide (MobileEntity other)
		{
			float difX = other.Velocity.X - Velocity.X;
			float difY = other.Velocity.X - Velocity.X;
			float difZ = other.Velocity.X - Velocity.X;
			this.Velocity = other.Velocity;
			Health = (int)(Health - (difX * difX + difY * difY + difZ * difZ));	
			if (Health < 0) {
				this.Destroy ();
			}

		}

	}

		

	public class LocalPlayer : Player
	{
		const long FIRE_INTERVAL = TimeSpan.TicksPerSecond * 3;

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
			Spawn ();
		}

		public void Spawn ()
		{
			Random rnd = new Random();
			Velocity = new Vector3 (0, 0, 0);
			Location = new Vector3(rnd.Next(0, 100), rnd.Next(0, 100), rnd.Next(0, 100));
		}

		public override void Destroy ()
		{
			Spawn ();
		}

		public Rocket Fire ()
		{
			if (DateTime.UtcNow.Ticks - LastShotFired >= FIRE_INTERVAL) {
				LastShotFired = DateTime.UtcNow.Ticks;
				return new Rocket (this.Location, this.Rotation);
			}
			return null;
		}

		public override void Collide (MobileEntity other)
		{
			float difX = other.Velocity.X - Velocity.X;
			float difY = other.Velocity.X - Velocity.X;
			float difZ = other.Velocity.X - Velocity.X;
			this.Velocity = other.Velocity;
			Health = (int)(Health - (difX * difX + difY * difY + difZ * difZ));	
			if (Health < 0) {
				this.Destroy ();
			}
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
