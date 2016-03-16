using System;

namespace Galaxpeer
{

	public class Vector3
	{
		public float X;
		public float Y;
		public float Z;

		public Vector3(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}
	}

	public class Vector4 : Vector3
	{
		public float W;

		public Vector4(float x, float y, float z, float w) : base(x, y, z)
		{
			W = w;
		}
		public static Vector4 operator * (Vector4 left,Vector4 right){
			Vector4 res = new Vector4 (left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
			res.normalize();
			return res;
		}
		public void normalize(){
			float length = (float)Math.Sqrt(X * X + Y * Y + Z * Z + W * W);
			X = X / length;
			Y = Y / length;
			Z = Z / length;
			W = W / length;
		}
		public Vector3 GetRightVector() 
		{
			return new Vector3( 1 - 2 * (Y * Y + Z * Z),
				2 * (X * Y + W * Z),
				2 * (X * Z - W * Y));
		}
		public  Vector3 GetUpVector(){
			return new Vector3( 2 * (X * Y - W * Z), 
				1 - 2 * (X * X + Z * Z),
				2 * (Y * Z + W * X));
		}
		public Vector3 GetForwardVector() 
		{
			return new Vector3( 2 * (X * Z + W * Y), 
				2 * (Y * X - W * X),
				1 - 2 * (X * X + Y * Y));
		}
	}

	public abstract class MobileEntity 
	{
		public Vector3 Location;
		public Vector4 Rotation;
		public Vector3 Velocity;
		public float size; 
		Guid ownedBy; 

		public abstract void collide(MobileEntity other);
		public abstract void destroy();
		public int Health;

		public void LocationUpdate(double stepsize){
	
			float nx =(float) (Location.X + stepsize * Velocity.X); 
				float ny =(float) (Location.Y + stepsize * Velocity.Y);
					float nz =(float) (Location.Z + stepsize * Velocity.Z);
			Location = new Vector3 (nx, ny, nz);
		}

		public void AccelerateForward(double stepsize, double acceleration,double maxspeed){
			float Vx =(float) (stepsize * (acceleration*Rotation.GetForwardVector().X));
			float Vy =(float) (stepsize * (acceleration*Rotation.GetForwardVector().Y));
			float Vz =(float) (stepsize * (acceleration*Rotation.GetForwardVector().Z));
			Velocity = new Vector3 (Vx, Vy, Vz);
			if ((Velocity.X * Velocity.X) + (Velocity.Y * Velocity.Y) + (Velocity.Z * Velocity.Z) > maxspeed*maxspeed) {
				Velocity = new Vector3((float)(Velocity.X / maxspeed),(float)(Velocity.Y / maxspeed),(float)(Velocity.Z / maxspeed));
			}
		}
		public void rotate(double up,double right,double spin){
			double ff = Math.Sin (up);
			double fr = Math.Sin (right);
			double fs = Math.Sin (spin);
			Vector3 rightVec = Rotation.GetRightVector (); 
			Vector3 upVec = Rotation.GetUpVector (); 
			Vector3 forVec = Rotation.GetForwardVector (); 

			double x = ff * rightVec.X;
			double y=ff*rightVec.Y;
			double z=ff*rightVec.Z;
			double w=Math.Cos(up/2.0);
			Vector4 r = new Vector4 ((float)x, (float)y, (float)z, (float)w);
			r.normalize();
			Rotation = Rotation*r;

			x = fr * upVec.X;
			y=fr*upVec.Y;
			z=fr*upVec.Z;
		    w=Math.Cos(right/2.0);
			r = new Vector4 ((float)x, (float)y, (float)z, (float)w);
			r.normalize();
			Rotation = Rotation*r;

			x = fs * forVec.X;
			y=fs*forVec.Y;
			z=fs*forVec.Z;
			w=Math.Cos(spin/2.0);
			r = new Vector4 ((float)x, (float)y, (float)z, (float)w);
			r.normalize();
			Rotation = Rotation * r;   
			Rotation.normalize ();
		}

		public bool CheckCollision(MobileEntity other){
			double Xdist = Location.X - other.Location.X;
			double Ydist = Location.Y - other.Location.Y;
			double Zdist = Location.Z - other.Location.Z;
			if (size + other.size > Math.Sqrt (Xdist * Xdist + Ydist * Ydist + Zdist * Zdist)) {
				return true;
			}
			return false;
				
		}
	}

		public class Player: MobileEntity{
			public override void collide(MobileEntity other){
				this.Velocity = other.Velocity;
			}
			public override void destroy(){
			
			}
		}
		public class Rocket:MobileEntity{
			public override void collide(MobileEntity other){
				float difX=other.Velocity.X-Velocity.X;
				float difY=other.Velocity.X-Velocity.X;
				float difZ=other.Velocity.X-Velocity.X;
				this.Velocity = other.Velocity;
				Health = (int)(Health - (difX * difX + difY * difY + difZ * difZ));
				other.Health = other.Health - 10;
				this.destroy ();
			}
			public override void destroy(){}
				
		}
		public class astroid : MobileEntity{
			public override void collide(MobileEntity other){
				float difX=other.Velocity.X-Velocity.X;
				float difY=other.Velocity.X-Velocity.X;
				float difZ=other.Velocity.X-Velocity.X;
				this.Velocity = other.Velocity;
				Health = (int)(Health - (difX * difX + difY * difY + difZ * difZ));	
				if (Health < 0) {
					this.destroy ();
				}

			}
			public override void destroy(){}

		}

		

	public class LocalPlayer : Player
	{
		public int Health;
		public long LastShotFired;

		private static volatile LocalPlayer instance;
		private static object syncRoot = new Object();

		private LocalPlayer() {}

		public override void destroy(){}

		public override void collide(MobileEntity other){
			float difX=other.Velocity.X-Velocity.X;
			float difY=other.Velocity.X-Velocity.X;
			float difZ=other.Velocity.X-Velocity.X;
			this.Velocity = other.Velocity;
			Health = (int)(Health - (difX * difX + difY * difY + difZ * difZ));	
			if (Health < 0) {
				this.destroy ();
			}
		}


		public static LocalPlayer Instance
		{
			get 
			{
				if (instance == null) 
				{
					lock (syncRoot) 
					{
						if (instance == null) 
							instance = new LocalPlayer();
					}
				}

				return instance;
			}
		}
	}
}