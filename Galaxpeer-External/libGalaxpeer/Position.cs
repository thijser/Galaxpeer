using System;
using System.Collections.Generic;

namespace Galaxpeer
{
	public static class Position
	{
		public static float ROI_RADIUS = 300;
		public static float SIGHT_DISTANCE = 100;

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

		public static bool IsInRoi(Vector3 myLocation, Vector3 otherLocation)
		{
			return GetDistance (myLocation, otherLocation) <= ROI_RADIUS;
		}

		public static bool IsInAnyRoi(ICollection<Client> clients, Vector3 location)
		{
			foreach (var client in clients) {
				if (IsInRoi (client.Player.Location, location)) {
					return true;
				}
			}

			return false;
		}

		public static bool IsInSight(Vector3 a, Vector3 b)
		{
			return GetDistance (a, b) <= SIGHT_DISTANCE;
		}
	}
}
