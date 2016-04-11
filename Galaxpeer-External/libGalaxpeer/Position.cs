using System;
using System.Collections.Generic;

namespace Galaxpeer
{
	public static class Position
	{
		public const float ROI_RADIUS = 150;
		public const float PLAYER_ROI_RADIUS = 2 * ROI_RADIUS;

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

		private static bool IsInRoi (Vector3 a, Vector3 b, float r)
		{
			return GetDistance (a, b) <= r;
		}

		public static bool IsEntityInRoi(Vector3 player, Vector3 entity)
		{
			return IsInRoi (player, entity, ROI_RADIUS);
		}

		public static bool IsEntityNearRoi (Vector3 player, Vector3 entity)
		{
			return IsInRoi (player, entity, ROI_RADIUS * 1.2f);
		}

		public static bool IsClientInRoi(Vector3 a, Vector3 b)
		{
			return IsInRoi (a, b, PLAYER_ROI_RADIUS);
		}

		public static bool IsClientInRoi(Vector3 location)
		{
			return IsClientInRoi (LocalPlayer.Instance.Location, location);
		}

		public static bool IsInAnyRoi(Vector3 location)
		{
			bool inRoi = false;
			Game.ConnectionManager.ClientsInRoi.ForEach ((Guid uuid, Client client) => {
				if (IsEntityInRoi(client.Player.Location, location)) {
					inRoi = true;
				}
			});
			return inRoi;
		}

		public static bool ClosestClient (Vector3 location, out Client client)
		{
			Client closest = null;
			double distance = Position.GetDistance (LocalPlayer.Instance.Location, location) - 30;
			Game.ConnectionManager.ClientsInRoi.ForEach ((Guid id, Client c) => {
				double d = Position.GetDistance(c.Player.Location, location);
				if (d < distance && d <= ROI_RADIUS) {
					closest = c;
					distance = d;
				}
			});
			client = closest;
			return client != null;
		}

		public static Client ClosestClient ()
		{
			Vector3 myLoc = LocalPlayer.Instance.Location;
			double distance = double.MaxValue;
			Client closest = null;
			foreach (var client in Game.ConnectionManager.ClosestClients) {
				if (client != null) {
					double d = Position.GetDistance (myLoc, client.Player.Location);
					if (d < distance) {
						distance = d;
						closest = client;
					}
				}
			}
			return closest;
		}

		static Random rnd = new Random();
		public static Vector3 Near (Vector3 location, float distance = Position.ROI_RADIUS - 10)
		{
			double s = rnd.NextDouble () % (2 * Math.PI);
			double t = rnd.NextDouble () % (2 * Math.PI);

			double x = Math.Cos (s) * Math.Cos (t);
			double y = Math.Sin (s) * Math.Cos (t);
			double z = Math.Sin (t);

			if (rnd.NextDouble () > .5) {
				distance *= -1;
			}

			return location + (new Vector3 ((float) x, (float) y, (float) z) * distance);
		}
	}
}
