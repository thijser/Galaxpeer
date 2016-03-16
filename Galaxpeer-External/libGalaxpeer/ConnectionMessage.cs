using System;
using System.Net;
using System.Text;

namespace Galaxpeer
{
	public class ConnectionMessage : Message
	{
		public static event MessageHandler OnReceive;

		public Guid UUID;
		public IPAddress Ip;
		public int Port;

		struct Packet
		{
			public char Id;
			public Guid UUID;
			public UInt32 Ip;
			public UInt16 Port;
		};

		public ConnectionMessage(Guid uuid, IPAddress ip, int port)
		{
			UUID = uuid;
			Ip = ip;
			Port = port;
		}

		public ConnectionMessage(Byte[] bytes)
		{
			Packet packet = FromBytes<Packet> (bytes);
			UUID = packet.UUID;
			Ip = new IPAddress (packet.Ip);
			Port = packet.Port;

			if (OnReceive != null) {
				OnReceive (this);
			}
		}

		public override byte[] Serialize()
		{
			Packet packet;
			packet.Id = 'C';
			packet.UUID = UUID;
			packet.Ip = BitConverter.ToUInt32(Ip.GetAddressBytes(), 0);
			packet.Port = (UInt16) Port;
			byte[] bytes = ToBytes (packet);

			return bytes;
		}
	}
}
