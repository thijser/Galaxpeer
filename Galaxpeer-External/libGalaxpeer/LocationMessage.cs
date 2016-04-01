using System;
using System.Net;

namespace Galaxpeer
{
	public class LocationMessage : TMessage<LocationMessage>
	{
		public byte Type { get; private set; }
		public Guid Uuid { get; private set; }
		public Vector3 Location { get; private set; }
		public Vector4 Rotation { get; private set; }
		public Vector3 Velocity { get; private set; }

		struct Packet
		{
			public char Id;
			public long Timestamp;
			public byte Type;
			public Guid Uuid;
			public Vector3 Location;
			public Vector4 Rotation;
			public Vector3 Velocity;
		}

		public LocationMessage(MobileEntity mob)
		{
			Type = (byte)mob.Type;
			Uuid = mob.Uuid;
			Location = mob.Location;
			Rotation = mob.Rotation;
			Velocity = mob.Velocity;
		}

		public LocationMessage(byte[] bytes)
		{
			Packet packet = FromBytes<Packet> (bytes);
			Timestamp = packet.Timestamp;
			Type = packet.Type;
			Uuid = packet.Uuid;
			Location = packet.Location;
			Rotation = packet.Rotation;
			Velocity = packet.Velocity;
		}

		public override byte[] Serialize()
		{
			Packet packet;
			packet.Id = 'L';
			packet.Timestamp = Timestamp;
			packet.Type = Type;
			packet.Uuid = Uuid;
			packet.Location = Location;
			packet.Rotation = Rotation;
			packet.Velocity = Velocity;

			return ToBytes (packet);
		}
	}
}
