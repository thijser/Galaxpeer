using System.Math;

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
		public Vector4 operator * (Vector4 left,Vector4 right){
			Vector4 res = new Vector4 (left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
			return res.normalize();
		}
		public void normalize(){
			double length = Math.sqrt(X * X + Y * Y + Z * Z + W * W);
			X = X / length;
			Y = Y / length;
			Z = Z / length;
			W = W / length;
		}
		public Vector3 GetRightVector() 
		{
			return Vector3( 1 - 2 * (y * y + z * z),
				2 * (x * y + w * z),
				2 * (x * z - w * y));
		}
		public Vector3 GetUpVector(){
			return Vector3( 2 * (x * y - w * z), 
				1 - 2 * (x * x + z * z),
				2 * (y * z + w * x));
		}
		public Vector3 GetForwardVector() 
		{
			return Vector3( 2 * (x * z + w * y), 
				2 * (y * x - w * x),
				1 - 2 * (x * x + y * y));
		}
	}

	public abstract class MobileEntity 
	{
		public Vector3 Location;
		public Vector4 Rotation;
		public Vector3 Velocity;
		public float size; 

		abstract void collide(MobileEntity other);

		public void LocationUpdate(double stepsize){
			Location = Location + stepsize * Velocity; 
		}
		public void AccelerateForward(double stepsize, double acceleration,double maxspeed){
			Velocity = Velocity + (stepsize * (acceleration*GetForwardVector()));

			if ((Velocity.X * Velocity.X) + (Velocity.Y * Velocity.Y) + (Velocity.Z * Velocity.Z) > maxspeed*maxspeed) {
				Velocity = Velocity / maxspeed;
			}
		}
		public void rotate(double up,double right,double spin){
			double ff = Math.sin (forward);
			double fr = Math.sin (right);
			double fs = Math.sin (spin);
			Vector3 rightVec = Rotation.GetRightVector (); 
			Vector3 upVec = Rotation.GetUpVector (); 
			Vector3 forVec = Rotation.GetForwardVector (); 

			double x = ff * rightVec.X;
			double y=ff*rightVec.Y;
			double z=ff*rightVec.Z;
			double w=Math.cos(up/2.0);
			Vector4 r = new Vector4 (x, y, z, w);
			r.normalize;
			Rotation = Rotation*r;

			x = fr * upVec.X;
			y=fr*upVec.Y;
			z=fr*upVec.Z;
		    w=Math.cos(right/2.0);
			r = new Vector4 (x, y, z, w);
			r.normalize;
			Rotation = Rotation*r;

			x = fs * forVec.X;
			y=fs*forVec.Y;
			z=fs*forVec.Z;
			w=Math.cos(spin/2.0);
			r = new Vector4 (x, y, z, w);
			r.normalize;
			Rotation = Rotation * r;   
			Rotation.normalize ();
		}

		public bool CheckCollision(MobileEntity other){
			double Xdist = Location.X - other.Location.X;
			double Ydist = Location.Y - other.Location.Y;
			double Zdist = Location.Z - other.Location.Z;
			if (size + other.size > math.sqrt (Xdist * Xdist + Ydist * Ydist + Zdist * Zdist)) {
				this.collide (other);
			}
				
		}

		public class Player: MobileEntity{
			
		}
		public class LocalPlayer : Player
		{
			public int Health;
			public long LastShotFired;
		}
	}
}
