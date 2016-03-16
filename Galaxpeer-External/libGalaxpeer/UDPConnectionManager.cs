using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
namespace Galaxpeer
{

	public class UDPConnectionManager : ConnectionManager
	{
		UdpClient socket;
		Hashtable handlers;

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
			//connection.Send (new LocationMessage (LocalPlayer.Instance));
		}

		protected void Receive()
		{
			socket.BeginReceive (new AsyncCallback (onReceive), null);
		}

		protected void onReceive(IAsyncResult result)
		{
			try
			{
				IPEndPoint ip = new IPEndPoint (IPAddress.Any, 0);
				Byte[] received = socket.EndReceive (result, ref ip);
				Message m = MessageFactory.Parse (received);
			}
			catch (Exception)
			{

			}
			Receive ();
		}
	}
}
