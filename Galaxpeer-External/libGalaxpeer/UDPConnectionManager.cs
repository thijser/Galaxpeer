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
			socket = new UdpClient (0);
			IPEndPoint endPoint = ((IPEndPoint)socket.Client.LocalEndPoint);
			port = ((IPEndPoint) socket.Client.LocalEndPoint).Port;
			LocalConnection = new ConnectionMessage(Guid.NewGuid(), 
			Receive ();
		}

		public void Connect(ConnectionMessage message)
		{
			connection = new UDPConnection (message);
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
