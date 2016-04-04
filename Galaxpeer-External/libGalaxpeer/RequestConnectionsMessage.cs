using System.Net;

namespace Galaxpeer
{
	public class RequestConnectionsMessage : TMessage<RequestConnectionsMessage>
	{
		public Vector3 Location { get; private set; }

		struct Packet
		{
			public char Id;
			public sbyte Hops;
			public long Timestamp;
			public Vector3 Location;
		};

		public RequestConnectionsMessage(Vector3 location)
		{
			Location = location;
		}

		public RequestConnectionsMessage(byte[] bytes)
		{
			Packet packet = FromBytes<Packet> (bytes);
			Hops = packet.Hops;
			Timestamp = packet.Timestamp;
			Location = packet.Location;
		}

		public override byte[] Serialize()
		{
			Packet packet;
			packet.Id = 'R';
			packet.Hops = Hops;
			packet.Timestamp = Timestamp;
			packet.Location = Location;
			return ToBytes (packet);
		}
	}
}
