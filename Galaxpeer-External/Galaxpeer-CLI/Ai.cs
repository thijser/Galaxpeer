
using Galaxpeer;
using System.Collections.Generic;
using System;

namespace GalaxpeerCLI
{
	public class Ai
	{
		//Player currentTarget;

		public Ai()
		{
			PsycicManager.OnTick += OnTick;
		}

		Vector3 rotation = new Vector3(0, 0, 0);
		void OnTick (long time)
		{
			/*if (currentTarget == null) {
				Player nearestTarget = null;
				double nearestDistance = double.MaxValue;
				foreach (var client in Game.ConnectionManager.ClosestClients) {
					if (Position.IsInSight (LocalPlayer.Instance.Location, client.Player.Location)) {
						double distance = Position.GetDistance (LocalPlayer.Instance.Location, client.Player.Location);
						if (distance < nearestDistance) {
							nearestTarget = client.Player;
							nearestDistance = distance;
						}
					}
				}
				currentTarget = nearestTarget;
			}*/
			UnityUnityInterface ui = (UnityUnityInterface) UnityInterfaceInterfaceManager.InstanceUnintyInterface;
			Random rnd = new Random ();
			rotation.X += rnd.Next (-10, 10) / 100;
			rotation.Y += rnd.Next (-10, 10) / 100;
			rotation.Z += rnd.Next (-10, 10) / 100;
			ui.rotateplayer (rotation);
			ui.shootplayer ();
			ui.accaleratePlayer (0.01);
		}
	}
}
