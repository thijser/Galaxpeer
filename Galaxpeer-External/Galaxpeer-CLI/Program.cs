using System;
using System.Net;
using Galaxpeer;
using System.Threading;

namespace GalaxpeerCLI
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			ConnectionManager c1 = new UDPConnectionManager ();
			Game.Init (c1);

			Console.WriteLine ("Listening on port {0}", c1.LocalConnectionMessage.Port);

			if (args.Length >= 2) {
				Console.WriteLine ("Connecting to {0}:{1}", args [0], args[1]);
				c1.Connect (new ConnectionMessage (new Guid (), IPAddress.Parse (args [0]), int.Parse (args [1]), LocalPlayer.Instance.Location));
			}

			//ConnectionManager c2 = new UDPConnectionManager ();
			//Game.Init (c2);
			//c2.Connect (new ConnectionMessage(new Guid(), IPAddress.Parse("127.0.0.1"), c1.LocalConnectionMessage.Port, LocalPlayer.Instance.Location));

			//ConnectionMessage m = new ConnectionMessage (System.Guid.NewGuid (), System.Net.IPAddress.Parse ("127.0.0.1"), 12346);
			//UDPConnectionManager connmanager = new UDPConnectionManager ();
			//connmanager.Connect (m);

			/*UDPConnection conn = new UDPConnection (m);
			conn.Send (m);*/

			new Timer (Tick, null, 0, 1000);


			/*while (true) {
				Thread.Sleep (5000);
				PsycicManager.Instance.addEntity(new Asteroid());
			}*/

			Thread.Sleep (Timeout.Infinite);
		}

		private static void Tick(object _)
		{
			PsycicManager.Instance.tick ();
		}
	}
}
