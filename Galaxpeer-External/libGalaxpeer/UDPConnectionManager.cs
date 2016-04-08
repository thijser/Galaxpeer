using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
namespace Galaxpeer
{

	public class UDPConnectionManager : ConnectionManager
	{
		UdpClient socket;
		public int portstore;
		public UDPConnectionManager(int port = 0)
		{
			socket = new UdpClient (port);
			IPEndPoint endPoint = (IPEndPoint) socket.Client.LocalEndPoint;
			this.LocalConnectionMessage = new ConnectionMessage (LocalPlayer.Instance.Uuid, new IPAddress(0), endPoint.Port, LocalPlayer.Instance.Location);
			portstore=endPoint.Port;
			Receive ();
		}

		public override Connection Connect(ConnectionMessage message)
		{
			Connection connection = new UDPConnection (message);
			this.LocalConnectionMessage.Location = LocalPlayer.Instance.Location;
			this.LocalConnectionMessage.Timestamp = DateTime.UtcNow.Ticks;
			connection.Send (this.LocalConnectionMessage);
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
			try
			{
				IPEndPoint ip = new IPEndPoint (IPAddress.Any, 0);
				Byte[] received = socket.EndReceive (result, ref ip);
				Message message = MessageFactory.Parse (received);
				message.Parsed();
				message.SourceIp = ip;
				message.SourceClient = GetByEndPoint (ip);
				if (message.SourceClient == null) {
					//throw new ArgumentException ("Message from unknown client");
				} else {
					if (message.GetType() != typeof(LocationMessage)) {
						Console.WriteLine ("Received {0} from {1}", (char)received [0], ip);
					}
					message.SourceClient.Update ();
					message.Dispatch();
				}
			}
			catch (Exception e)
			{
				Console.WriteLine (e.ToString());
			}
			Receive ();
		}
	}
}
