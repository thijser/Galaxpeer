using System;
using System.Net;
using System.Text;
using System.Runtime.InteropServices;

namespace Galaxpeer
{
	public class ConnectionMessage : TMessage<ConnectionMessage>, ILocationMessage
	{
		public static long MAX_AGE = 3 * TimeSpan.TicksPerMinute;

		public static event MessageHandler<ConnectionMessage> OnParse;

		public override IPEndPoint SourceIp
		{
			set {
				if (Ip.Equals (new IPAddress (0))) {
					Ip = value.Address;

					// Connection belonging to endpoint, so add to manager
					if (Game.ConnectionManager.GetByEndPoint (value) == null) {
						Game.ConnectionManager.AddByEndPoint (value, this);
					}
				}
				base.SourceIp = value;
			}
		}

		public Guid Uuid { get; private set; }
		public IPAddress Ip;
		public int Port;
		public Vector3 Location { get; set; }

		[StructLayout(LayoutKind.Sequential, Pack=1, CharSet=CharSet.Unicode)]
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

			Game.ConnectionManager.ConnectionCache.Update (Uuid);

			if (OnParse != null) {
				OnParse (this);
			}
		}

		public override byte[] Serialize()
		{
			var player = EntityManager.Get (Uuid);
			if (player != null) {
				Location = player.Location;
			}

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
	}
}
