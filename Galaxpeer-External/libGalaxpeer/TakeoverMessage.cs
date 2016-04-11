using System;
using System.Runtime.InteropServices;

namespace Galaxpeer
{
	class TakeoverMessage : TMessage<TakeoverMessage>
	{
		public Guid ObjectUuid;
		public Guid OwnerUuid;

		[StructLayout(LayoutKind.Sequential, Pack=1, CharSet=CharSet.Unicode)]
		struct Packet
		{
			public char Id;
			public long Timestamp;
			public Guid ObjectUuid;
			public Guid OwnerUuid;
		}

		public TakeoverMessage(MobileEntity mob)
		{
			ObjectUuid = mob.Uuid;
			OwnerUuid = LocalPlayer.Instance.Uuid;
		}

		public TakeoverMessage(byte[] bytes)
		{
			Packet packet = FromBytes<Packet> (bytes);
			Timestamp = packet.Timestamp;
			ObjectUuid = packet.ObjectUuid;
			OwnerUuid = packet.OwnerUuid;
		}

		public override byte[] Serialize()
		{
			Packet packet;
			packet.Id = 'T';
			packet.Timestamp = Timestamp;
			packet.ObjectUuid = ObjectUuid;
			packet.OwnerUuid = OwnerUuid;

			return ToBytes (packet);
		}
	}
}

