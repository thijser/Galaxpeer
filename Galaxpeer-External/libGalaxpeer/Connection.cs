using System;
using System.Net;
using System.Net.Sockets;

namespace Galaxpeer
{
	public abstract class Connection
	{
		public Guid UUID;
		public IPAddress Ip;
		public int Port;

		public Connection(ConnectionMessage message)
		{
			UUID = message.UUID;
			Ip = message.Ip;
			Port = message.Port;
		}

		public Connection(IPAddress ip, int port)
		{
			Ip = ip;
			Port = port;
		}

		public abstract void Send (Message message);
	}
}
