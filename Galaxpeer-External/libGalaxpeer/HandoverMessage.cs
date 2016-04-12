using System;
using System.Net;
using System.Runtime.InteropServices;

namespace Galaxpeer
{
	public class HandoverMessage : TMessage<HandoverMessage>, IFullLocationMessage
	{
		public override char Id { get { return 'H'; } }
		public MobileEntity.EntityType Type { get; private set; }
		public Guid Uuid { get; private set; }
		public Guid OwnedBy { get { return LocalPlayer.Instance.Uuid; } }
		public Vector3 Location { get; private set; }
		public Quaternion Rotation { get; private set; }
		public Vector3 Velocity { get; private set; }
		public int Health { get; private set; }

		[StructLayout(LayoutKind.Sequential, Pack=1, CharSet=CharSet.Unicode)]
		struct Packet
		{
			public char Id;
			public byte Type;
			public short Health;
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
			Health = mob.Health;
		}

		public HandoverMessage(byte[] bytes)
		{
			Packet packet = FromBytes<Packet> (bytes);
			Timestamp = packet.Timestamp;
			Health = packet.Health;
			Type = (MobileEntity.EntityType) packet.Type;
			Uuid = packet.Uuid;
			Location = packet.Location;
			Rotation = packet.Rotation;
			Velocity = packet.Velocity;
		}

		public override byte[] Serialize()
		{
			Packet packet;
			packet.Id = Id;
			packet.Timestamp = Timestamp;
			packet.Health = (short)Health;
			packet.Type = (byte) Type;
			packet.Uuid = Uuid;
			packet.Location = Location;
			packet.Rotation = Rotation;
			packet.Velocity = Velocity;

			return ToBytes (packet);
		}
	}
}
