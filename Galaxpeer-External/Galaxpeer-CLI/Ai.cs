
using Galaxpeer;
using System.Collections.Generic;
using System;
using System.Diagnostics;
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
			rotation.X += (float)rnd.Next (-10, 10) / 10.0f;
			rotation.Y += (float)rnd.Next (-10, 10) / 10.0f;
			rotation.Z += (float)rnd.Next (-10, 10) / 10.0f;
			Debug.Print (rotation.ToString());
			ui.rotateplayer (rotation);
			ui.shootplayer ();
			ui.accaleratePlayer (0.01);
		}
	}
}
