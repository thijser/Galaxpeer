using System;

namespace Galaxpeer
{
	class HandoverMessage : TMessage<HandoverMessage>
	{
		Guid ObjectUuid;
		Guid OwnerUuid;

		[StructLayout(LayoutKind.Sequential, Pack=1, CharSet=CharSet.Unicode)]
		struct Packet
		{
			public char Id;
			public sbyte Hops;
			public long Timestamp;
			public Guid ObjectUuid;
			public Guid OwnerUuid;
		}

		public HandoverMessage(MobileEntity mob)
		{
			ObjectUuid = mob.Uuid;
			OwnerUuid = mob.OwnedBy;
		}

		public HandoverMessage(byte[] bytes)
		{
			Packet packet = FromBytes<Packet> (bytes);
			Timestamp = packet.Timestamp;
			Hops = packet.Hops;
			ObjectUuid = packet.ObjectUuid;
			OwnerUuid = packet.OwnerUuid;
		}

		public override byte[] Serialize()
		{
			Packet packet;
			packet.Id = 'H';
			packet.Hops = Hops;
			packet.Timestamp = Timestamp;
			packet.ObjectUuid = ObjectUuid;
			packet.OwnerUuid = OwnerUuid;

			return ToBytes (packet);
		}
	}
}
