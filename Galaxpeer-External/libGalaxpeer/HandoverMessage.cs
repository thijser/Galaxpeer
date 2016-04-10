using System;
using System.Net;
using System.Runtime.InteropServices;

namespace Galaxpeer
{
	public class HandoverMessage : TMessage<HandoverMessage>, IFullLocationMessage
	{
		public MobileEntity.EntityType Type { get; private set; }
		public Guid Uuid { get; private set; }
		public Guid OwnedBy { get { return LocalPlayer.Instance.Uuid; } }
		public Vector3 Location { get; private set; }
		public Quaternion Rotation { get; private set; }
		public Vector3 Velocity { get; private set; }

		[StructLayout(LayoutKind.Sequential, Pack=1, CharSet=CharSet.Unicode)]
		struct Packet
		{
			public char Id;
			public sbyte Hops;
			public byte Type;
			public long Timestamp;
			public Guid Uuid;
			public Vector3 Location;
			public Quaternion Rotation;
			public Vector3 Velocity;
		}

		public HandoverMessage(MobileEntity mob)
		{
			Type = mob.Type;
			Uuid = mob.Uuid;
			Location = mob.Location;
			Rotation = mob.Rotation;
			Velocity = mob.Velocity;
		}

		public HandoverMessage(byte[] bytes)
		{
			Packet packet = FromBytes<Packet> (bytes);
			Timestamp = packet.Timestamp;
			Hops = packet.Hops;
			Type = (MobileEntity.EntityType) packet.Type;
			Uuid = packet.Uuid;
			Location = packet.Location;
			Rotation = packet.Rotation;
			Velocity = packet.Velocity;
		}

		public override byte[] Serialize()
		{
			Packet packet;
			packet.Id = 'H';
			packet.Hops = Hops;
			packet.Timestamp = Timestamp;
			packet.Type = (byte) Type;
			packet.Uuid = Uuid;
			packet.Location = Location;
			packet.Rotation = Rotation;
			packet.Velocity = Velocity;

			return ToBytes (packet);
		}
	}
}
