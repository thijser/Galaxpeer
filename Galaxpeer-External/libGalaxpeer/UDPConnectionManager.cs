using System;
using System.Net.Sockets;
using System.Net;

namespace Galaxpeer
{
	public class UDPConnectionManager : ConnectionManager
	{
		UdpClient socket;

		public UDPConnectionManager(int port = 12346)
		{
			socket = new UdpClient (port);
			Receive ();
		}

		protected void Receive()
		{
			Console.WriteLine ("Receiving");
			socket.BeginReceive (new AsyncCallback (onReceive), null);
		}

		protected void onReceive(IAsyncResult result)
		{
			IPEndPoint ip = new IPEndPoint (IPAddress.Any, 0);
			Byte[] received = socket.EndReceive (result, ref ip);
			new ConnectionMessage (received);
			Receive ();
		}
	}
}
