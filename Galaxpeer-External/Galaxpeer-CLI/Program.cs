using System;
using System.Net;
using Galaxpeer;
using System.Threading;

namespace GalaxpeerCLI
{
	class MainClass
	{
		static Ai ai;

		public static void Main (string[] args)
		{
			Config conf = new Config (args);

			ConnectionManager c1 = new UDPConnectionManager (conf.ListenPort);
			Console.WriteLine ("Listening on port {0}", c1.LocalConnectionMessage.Port);
			Game.Init (conf, c1);
			Console.WriteLine ("Connecting to {0}:{1}", conf.ConnectIP, conf.ConnectPort);

			Console.WriteLine ("Local player is {0}", LocalPlayer.Instance.Uuid);

			ai = new Ai ();
			//new Timer (Tick, null, 0, 1000);
			Thread.Sleep (Timeout.Infinite);
		}

		private static void Tick(object _)
		{
			var rocket = LocalPlayer.Instance.Fire ();
			if (rocket != null) {
				PsycicManager.Instance.AddEntity (rocket);
			}
		}
	}
}
