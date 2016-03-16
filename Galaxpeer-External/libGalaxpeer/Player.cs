
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
	}

	public class Player
	{
		public Vector3 Location;
		public Vector4 Rotation;
		public Vector3 Velocity;
	}

	public class LocalPlayer : Player
	{
		public int Health;
		public long LastShotFired;
	}
}
