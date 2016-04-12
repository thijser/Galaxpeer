
using System;
using System.Net;
using System.Net.Sockets;

namespace Galaxpeer
{
	public class UDPConnection : Connection
	{
		public static UdpClient socket;
		IPEndPoint EndPoint;

		public UDPConnection(ConnectionMessage message) : base(message)
		{
			//socket = new UdpClient ();
			//socket.Connect (message.Ip, message.Port);
			EndPoint = new IPEndPoint(message.Ip, message.Port);
		}

		public override void Send(Message message)
		{
			if (message != null) {
				byte[] serialized = message.Serialize ();
				socket.BeginSend (serialized, serialized.Length, EndPoint, new AsyncCallback (onSend), null);
			}
		}

		public override void Close ()
		{
			// TODO implement UDPConnection.Close()
			//Console.WriteLine ("TODO: close connection");
		}

		protected void onSend(IAsyncResult result)
		{
			try
			{
				socket.EndSend (result);
			}
			catch (Exception)
			{
				// TODO connection refused
			}
		}
	}
}
