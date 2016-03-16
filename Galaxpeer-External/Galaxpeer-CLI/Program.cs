using System;
using Galaxpeer;
using System.Threading;

namespace GalaxpeerCLI
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("Hello World!");
			new UDPConnectionManager ();

			ConnectionMessage m = new ConnectionMessage (System.Guid.NewGuid (), System.Net.IPAddress.Parse ("127.0.0.1"), 12346);
			UDPConnection conn = new UDPConnection (m);
			conn.Send (m);
			Thread.Sleep (Timeout.Infinite);
		}
	}
}
