using System;
using System.Net;
using System.Text;

namespace Galaxpeer
{
	public class ConnectionMessage : Message
	{
		public static event MessageHandler<ConnectionMessage> OnReceive;

		public Guid Uuid;
		public IPAddress Ip;
		public int Port;

		struct Packet
		{
			public char Id;
			public Guid Uuid;
			public UInt32 Ip;
			public UInt16 Port;
		};

		public ConnectionMessage(Guid uuid, IPAddress ip, int port)
		{
			Uuid = uuid;
			Ip = ip;
			Port = port;
		}

		public ConnectionMessage(Byte[] bytes)
		{
			Packet packet = FromBytes<Packet> (bytes);
			Uuid = packet.Uuid;
			Ip = new IPAddress (packet.Ip);
			Port = packet.Port;
		}

		public override byte[] Serialize()
		{
			Packet packet;
			packet.Id = 'C';
			packet.Uuid = Uuid;
			packet.Ip = BitConverter.ToUInt32(Ip.GetAddressBytes(), 0);
			packet.Port = (UInt16) Port;
			byte[] bytes = ToBytes (packet);

			return bytes;
		}

		public override void DispatchFrom(IPEndPoint endPoint)
		{
			if (Ip.Equals (new IPAddress (0))) {
				Ip = endPoint.Address;

				// Connection belonging to endpoint, so add to manager
				Game.ConnectionManager.AddByEndPoint(endPoint, this);
			}

			if (OnReceive != null) {
				Client client = Game.ConnectionManager.GetByEndPoint (endPoint);
				if (client != null) {
					OnReceive (client, this);
				}
			}
		}
	}
}
