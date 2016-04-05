using System;
using System.Runtime.InteropServices;

namespace Galaxpeer
{
	[StructLayout (LayoutKind.Sequential)]
	public class Vector3
	{
		public float X;
		public float Y;
		public float Z;

		public Vector3 (float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public static Vector3 operator + (Vector3 a, Vector3 b)
		{
			return new Vector3 (a.X + b.X, a.Y + b.Y, a.Z + b.Z);
		}

		public static Vector3 operator - (Vector3 a, Vector3 b)
		{
			return new Vector3 (a.X - b.X, a.Y - b.Y, a.Z - b.Z);
		}

		public static Vector3 operator * (Vector3 v, float d)
		{
			return new Vector3 (v.X * d, v.Y * d, v.Z * d);
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	public class Vector4
	{
		public float X;
		public float Y;
		public float Z;
		public float W;

		public Vector4 (float x, float y, float z, float w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		public static Vector4 operator * (Vector4 left, Vector4 right)
		{
			Vector4 res = new Vector4 (left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
			res.normalize ();
			return res;
		}

		public void normalize ()
		{
			float length = (float)Math.Sqrt (X * X + Y * Y + Z * Z + W * W);
			X = X / length;
			Y = Y / length;
			Z = Z / length;
			W = W / length;
		}

		public Vector3 GetRightVector ()
		{
			return new Vector3 (1 - 2 * (Y * Y + Z * Z),
				2 * (X * Y + W * Z),
				2 * (X * Z - W * Y));
		}

		public  Vector3 GetUpVector ()
		{
			return new Vector3 (2 * (X * Y - W * Z), 
				1 - 2 * (X * X + Z * Z),
				2 * (Y * Z + W * X));
		}

		public Vector3 GetForwardVector ()
		{
			return new Vector3 (2 * (X * Z + W * Y), 
				2 * (Y * X - W * X),
				1 - 2 * (X * X + Y * Y));
		}
	}

	public delegate void EntityUpdateHandler(MobileEntity entity, bool owned);

	public abstract class MobileEntity
	{
		public static event EntityUpdateHandler OnLocationUpdate;
		public static event EntityUpdateHandler OnDestroy;

		public enum EntityType : byte { Player = 1, Rocket = 2, Asteroid = 3 };

		protected Vector3 location;
		protected Vector4 rotation;
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

		public Vector4 Rotation
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
		public long LastUpdate;

		public MobileEntity ()
		{
			Uuid = Guid.NewGuid ();
			OwnedBy = LocalPlayer.LocalUuid;
			location = new Vector3 (0, 0, 0);
			rotation = new Vector4 (0, 0, 0, 1);
			velocity = new Vector3 (0, 0, 0);
			Size = 0;
			Console.WriteLine ("Generated MobileEntity {0} of type {1} at {2}.{3}.{4}", Uuid, this.Type, Location.X, Location.Y, Location.Z);
		}

		public MobileEntity(LocationMessage message)
		{
			Uuid = message.Uuid;
			OwnedBy = message.SourceClient.Uuid;
			copyMessageData (message);
			Size = 0;
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

		public void Destroy ()
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
			Location = new Vector3 (nx, ny, nz);

			if (!Position.IsInAnyRoi (Game.ConnectionManager.ClientsInRoi.Values, Location) && !Position.IsInRoi(LocalPlayer.Instance.Location, Location)) {
				Destroy ();
			}
		}

		public void AccelerateForward (double stepsize, double acceleration, double maxspeed)
		{
			float Vx = (float)(stepsize * (acceleration * Rotation.GetForwardVector ().X));
			float Vy = (float)(stepsize * (acceleration * Rotation.GetForwardVector ().Y));
			float Vz = (float)(stepsize * (acceleration * Rotation.GetForwardVector ().Z));
			velocity = new Vector3 (Vx, Vy, Vz);
			if ((Velocity.X * Velocity.X) + (Velocity.Y * Velocity.Y) + (Velocity.Z * Velocity.Z) > maxspeed * maxspeed) {
				velocity = new Vector3 ((float)(Velocity.X / maxspeed), (float)(Velocity.Y / maxspeed), (float)(Velocity.Z / maxspeed));
			}
			fireUpdate (true);
		}

		public void Rotate (double up, double right, double spin)
		{
			double ff = Math.Sin (up);
			double fr = Math.Sin (right);
			double fs = Math.Sin (spin);
			Vector3 rightVec = Rotation.GetRightVector (); 
			Vector3 upVec = Rotation.GetUpVector (); 
			Vector3 forVec = Rotation.GetForwardVector (); 

			double x = ff * rightVec.X;
			double y = ff * rightVec.Y;
			double z = ff * rightVec.Z;
			double w = Math.Cos (up / 2.0);
			Vector4 r = new Vector4 ((float)x, (float)y, (float)z, (float)w);
			r.normalize ();
			rotation = Rotation * r;

			x = fr * upVec.X;
			y = fr * upVec.Y;
			z = fr * upVec.Z;
			w = Math.Cos (right / 2.0);
			r = new Vector4 ((float)x, (float)y, (float)z, (float)w);
			r.normalize ();
			rotation = Rotation * r;

			x = fs * forVec.X;
			y = fs * forVec.Y;
			z = fs * forVec.Z;
			w = Math.Cos (spin / 2.0);
			r = new Vector4 ((float)x, (float)y, (float)z, (float)w);
			r.normalize ();
			rotation = Rotation * r;   
			rotation.normalize ();
			fireUpdate (true);
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

		const float VELOCITY = 20;

		public Rocket(Vector3 location, Vector4 rotation)
		{
			this.location = location;
			this.rotation = rotation;

			Vector3 v = rotation.GetForwardVector ();

			float Vx = (float)(20 * v.X);
			float Vy = (float)(20 * v.Y);
			float Vz = (float)(20 * v.Z);
			Velocity = new Vector3 (Vx, Vy, Vz);
			//fireUpdate (true);
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

		public Asteroid()
		{
			Random rnd = new Random ();
			double s = rnd.NextDouble () % 2 * Math.PI;
			double t = rnd.NextDouble () % 2 * Math.PI;

			double x = Math.Cos (s) * Math.Cos (t);
			double y = Math.Sin (s) * Math.Cos (t);
			double z = Math.Sin (t);

			float distance = Position.ROI_RADIUS - 1;

			Location = LocalPlayer.Instance.Location + (new Vector3 ((float) x, (float) y, (float) z) * distance);
			Vector3 myLoc = LocalPlayer.Instance.Location;

			Rotate (rnd.NextDouble (), rnd.NextDouble(), rnd.NextDouble());
			Velocity = new Vector3 (rnd.Next (0, 20), rnd.Next (0, 20), rnd.Next (0, 20));
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
		const long FIRE_INTERVAL = TimeSpan.TicksPerSecond;

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
			Location = new Vector3(rnd.Next(0, 100), rnd.Next(0, 100), rnd.Next(0, 100));
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
							//EntityManager.Entities [instance.Uuid] = instance;
						}
					}
				}

				return instance;
			}
		}
	}
}
