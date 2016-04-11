using System.Net;
using System.Runtime.InteropServices;

namespace Galaxpeer
{
	public class RequestConnectionsMessage : TMessage<RequestConnectionsMessage>
	{
		public Vector3 Location { get; private set; }

		[StructLayout(LayoutKind.Sequential, Pack=1, CharSet=CharSet.Unicode)]
		struct Packet
		{
			public char Id;
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
			Timestamp = packet.Timestamp;
			Location = packet.Location;
		}

		public override byte[] Serialize()
		{
			Packet packet;
			packet.Id = 'R';
			packet.Timestamp = Timestamp;
			packet.Location = Location;
			return ToBytes (packet);
		}
	}
}
