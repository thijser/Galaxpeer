﻿using System.Net.Sockets;
using System;

namespace Galaxpeer
{
	public class UDPConnection : Connection
	{
		UdpClient socket;

		public UDPConnection(ConnectionMessage message) : base(message)
		{
			socket = new UdpClient ();
			socket.Connect (message.Ip, message.Port);
		}

		public override void Send(Message message)
		{
			byte[] serialized = message.Serialize ();
			socket.BeginSend (serialized, serialized.Length, new AsyncCallback(onSend), null);
		}

		protected void onSend(IAsyncResult result)
		{
			socket.EndSend (result);
		}
	}
}
