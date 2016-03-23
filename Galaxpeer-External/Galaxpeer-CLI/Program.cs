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
			ConnectionManager c2 = new UDPConnectionManager ();
			Game.Init (c2);
			c2.Connect (new ConnectionMessage(new Guid(), IPAddress.Parse("127.0.0.1"), c1.LocalConnectionMessage.Port));


			//ConnectionMessage m = new ConnectionMessage (System.Guid.NewGuid (), System.Net.IPAddress.Parse ("127.0.0.1"), 12346);
			//UDPConnectionManager connmanager = new UDPConnectionManager ();
			//connmanager.Connect (m);

			/*UDPConnection conn = new UDPConnection (m);
			conn.Send (m);*/
			Thread.Sleep (Timeout.Infinite);
		}
	}
}
