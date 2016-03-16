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
			IPEndPoint endPoint = (IPEndPoint) socket.Client.LocalEndPoint;
			this.LocalConnectionMessage = new ConnectionMessage (Guid.NewGuid (), endPoint.Address, endPoint.Port);
			Receive ();
		}

		public void Connect(ConnectionMessage message)
		{
			Connection connection = new UDPConnection (message);
			this.Connections.Add (connection);
			connection.Send (this.LocalConnectionMessage);
		}

		protected void Receive()
		{
			Console.WriteLine ("Receiving");
			socket.BeginReceive (new AsyncCallback (onReceive), null);
		}

		protected void onReceive(IAsyncResult result)
		{
			try
			{
				IPEndPoint ip = new IPEndPoint (IPAddress.Any, 0);
				Byte[] received = socket.EndReceive (result, ref ip);
				Message m = MessageFactory.Parse (received);
				Console.WriteLine (m);
			}
			catch (Exception)
			{

			}
			Receive ();
		}
	}
}
