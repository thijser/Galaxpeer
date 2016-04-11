using System;
using System.Runtime.InteropServices;

namespace Galaxpeer
{
	public class DestroyMessage : TMessage<DestroyMessage>
	{
		public Guid Uuid;

		[StructLayout(LayoutKind.Sequential, Pack=1, CharSet=CharSet.Unicode)]
		struct Packet
		{
			public char Id;
			public long Timestamp;
			public Guid Uuid;
		}

		public DestroyMessage(MobileEntity entity)
		{
			Uuid = entity.Uuid;
		}

		public DestroyMessage(byte[] bytes)
		{
			Packet packet = FromBytes<Packet> (bytes);
			Timestamp = packet.Timestamp;
			Uuid = packet.Uuid;
		}

		public override byte[] Serialize()
		{
			Packet packet;
			packet.Id = 'D';
			packet.Timestamp = Timestamp;
			packet.Uuid = Uuid;

			return ToBytes (packet);
		}
	}
}
