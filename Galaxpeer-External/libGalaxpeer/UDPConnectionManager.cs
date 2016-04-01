using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
namespace Galaxpeer
{

	public class UDPConnectionManager : ConnectionManager
	{
		UdpClient socket;

		public UDPConnectionManager(int port = 12346)
		{
			socket = new UdpClient (0);
			IPEndPoint endPoint = (IPEndPoint) socket.Client.LocalEndPoint;
			this.LocalConnectionMessage = new ConnectionMessage (Guid.NewGuid (), new IPAddress(0), endPoint.Port, LocalPlayer.Instance.Location);
			Receive ();
		}

		public override Connection Connect(ConnectionMessage message)
		{
			Connection connection = new UDPConnection (message);
			//this.Connections.Add (connection);
			this.LocalConnectionMessage.Timestamp = DateTime.UtcNow.Ticks;
			connection.Send (this.LocalConnectionMessage);
			connection.Send (new LocationMessage (LocalPlayer.Instance));
			// Request closest connections, until we reach a stable state
			connection.Send (new RequestConnectionsMessage(LocalPlayer.Instance.Location));
			return connection;
		}

		protected void Receive()
		{
			socket.BeginReceive (new AsyncCallback (onReceive), null);
		}

		protected void onReceive(IAsyncResult result)
		{
			//try
			//{
				IPEndPoint ip = new IPEndPoint (IPAddress.Any, 0);
				Byte[] received = socket.EndReceive (result, ref ip);
				Message message = MessageFactory.Parse (received);
				Console.WriteLine ("Received {0} from {1}", (char) received [0], ip);
				message.DispatchFrom (ip);
	//}
			//catch (Exception e)
			//{

			//}
			Receive ();
		}
	}
}
