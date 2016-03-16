using System;
using System.Net;

namespace Galaxpeer
{
	public class ConnectionMessage : Message
	{
		public Guid UUID;
		public IPAddress IP;
		public ushort Port;

		public ConnectionMessage(Guid uuid, IPAddress ip, ushort port)
		{
			UUID = uuid;
			IP = ip;
			Port = port;
		}

		public ConnectionMessage(Byte[] bytes)
		{
			Console.WriteLine(BytesToString (bytes));
		}

		public override byte[] Serialize()
		{
			return StringToBytes ("hello");
		}
	}
}
