using System;
using System.Runtime.InteropServices;

namespace Galaxpeer
{
	class RequestLocationMessage : TMessage<RequestLocationMessage>
	{
		public override sbyte max_hops { get { return 0; } }

		public Guid Uuid;

		[StructLayout(LayoutKind.Sequential, Pack=1, CharSet=CharSet.Unicode)]
		struct Packet
		{
			public char Id;
			public sbyte Hops;
			public long Timestamp;
			public Guid Uuid;
		}

		public RequestLocationMessage(Guid uuid)
		{
			Uuid = uuid;
		}

		public RequestLocationMessage(byte[] bytes)
		{
			Packet packet = FromBytes<Packet> (bytes);
			Timestamp = packet.Timestamp;
			Hops = packet.Hops;
			Uuid = packet.Uuid;
		}

		public override byte[] Serialize()
		{
			Packet packet;
			packet.Id = 'Q';
			packet.Hops = Hops;
			packet.Timestamp = Timestamp;
			packet.Uuid = Uuid;

			return ToBytes (packet);
		}
	}
}
