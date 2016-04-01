using System;
using System.Net;
using System.Text;

namespace Galaxpeer
{
	public class ConnectionMessage : TMessage<ConnectionMessage>
	{
		public static long MAX_AGE = 3 * TimeSpan.TicksPerMinute;

		public Guid Uuid;
		public IPAddress Ip;
		public int Port;
		public Vector3 Location;

		struct Packet
		{
			public char Id;
			public sbyte Hops;
			public long Timestamp;
			public Guid Uuid;
			public UInt32 Ip;
			public UInt16 Port;
			public Vector3 Location;
		};

		public ConnectionMessage(Guid uuid, IPAddress ip, int port, Vector3 location)
		{
			Uuid = uuid;
			Ip = ip;
			Port = port;
			Location = location;
		}

		public ConnectionMessage(Byte[] bytes)
		{
			Packet packet = FromBytes<Packet> (bytes);
			Hops = packet.Hops;
			Uuid = packet.Uuid;
			Ip = new IPAddress (packet.Ip);
			Port = packet.Port;
			Location = packet.Location;
			Timestamp = packet.Timestamp;
		}

		public override byte[] Serialize()
		{
			Packet packet;
			packet.Id = 'C';
			packet.Hops = Hops;
			packet.Timestamp = Timestamp;
			packet.Uuid = Uuid;
			packet.Ip = BitConverter.ToUInt32(Ip.GetAddressBytes(), 0);
			packet.Port = (UInt16) Port;
			packet.Location = Location;
			byte[] bytes = ToBytes (packet);

			bytes = ToBytes (packet);

			return bytes;
		}

		public override void DispatchFrom(IPEndPoint endPoint)
		{
			if (Ip.Equals (new IPAddress (0))) {
				Ip = endPoint.Address;

				// Connection belonging to endpoint, so add to manager
				if (Game.ConnectionManager.GetByEndPoint (endPoint) == null) {
					Game.ConnectionManager.AddByEndPoint (endPoint, this);
				}
			}

			if (EntityManager.Get (Uuid) == null) {
				EntityManager.Entities[Uuid] = new Player (this);
			}

			base.DispatchFrom (endPoint);
		}
	}
}
