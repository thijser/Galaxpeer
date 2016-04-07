using System;
using System.Runtime.InteropServices;

namespace Galaxpeer
{
	class HeartbeatMessage : TMessage<HeartbeatMessage>
	{
		public override sbyte max_hops { get { return 0; } }

		[StructLayout(LayoutKind.Sequential, Pack=1, CharSet=CharSet.Unicode)]
		struct Packet
		{
			public char Id;
			public sbyte Hops;
			public long Timestamp;
		}

		public HeartbeatMessage() {}

		public HeartbeatMessage(byte[] bytes)
		{
			Packet packet = FromBytes<Packet> (bytes);
			Timestamp = packet.Timestamp;
			Hops = packet.Hops;
		}

		public override byte[] Serialize()
		{
			Packet packet;
			packet.Id = 'B';
			packet.Hops = Hops;
			packet.Timestamp = Timestamp;

			return ToBytes (packet);
		}
	}
}

