using System;

namespace Galaxpeer
{
	public static class Position
	{
		public static int GetOctant (Vector3 myLocation, Vector3 otherLocation)
		{
			int octant = 0;

			if (otherLocation.X >= myLocation.X) {
				octant |= 1;
			}
			if (otherLocation.Y >= myLocation.Y) {
				octant |= 2;
			}
			if (otherLocation.Z >= myLocation.Z) {
				octant |= 4;
			}
			return octant;
		}

		public static double GetDistance (Vector3 myLocation, Vector3 otherLocation)
		{
			Vector3 d = otherLocation - myLocation;
			return Math.Sqrt (d.X * d.X + d.Y * d.Y + d.Z * d.Z);
		}
	}
}
