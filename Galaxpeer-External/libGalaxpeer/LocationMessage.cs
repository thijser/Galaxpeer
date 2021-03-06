﻿using System;
using System.Net;
using System.Runtime.InteropServices;

namespace Galaxpeer
{
	public class LocationMessage : TMessage<LocationMessage>, IFullLocationMessage
	{
		public override char Id { get { return 'L'; } }

		public MobileEntity.EntityType Type { get; private set; }
		public Guid Uuid { get; private set; }
		public Guid OwnedBy { get; private set; }
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
			public Guid OwnedBy;
			public Vector3 Location;
			public Quaternion Rotation;
			public Vector3 Velocity;
		}

		public LocationMessage(MobileEntity mob)
		{
			Type = mob.Type;
			Uuid = mob.Uuid;
			OwnedBy = mob.OwnedBy;
			Location = mob.Location;
			Rotation = mob.Rotation;
			Velocity = mob.Velocity;
			Health = mob.Health;
		}

		public LocationMessage(byte[] bytes)
		{
			Packet packet = FromBytes<Packet> (bytes);
			Timestamp = packet.Timestamp;
			Health = packet.Health;
			Type = (MobileEntity.EntityType) packet.Type;
			Uuid = packet.Uuid;
			OwnedBy = packet.OwnedBy;
			Location = packet.Location;
			Rotation = packet.Rotation;
			Velocity = packet.Velocity;
		}

		public override byte[] Serialize()
		{
			Packet packet;
			packet.Id = Id;
			packet.Health = (short) Health;
			packet.Timestamp = Timestamp;
			packet.Type = (byte) Type;
			packet.Uuid = Uuid;
			packet.OwnedBy = OwnedBy;
			packet.Location = Location;
			packet.Rotation = Rotation;
			packet.Velocity = Velocity;

			return ToBytes (packet);
		}
	}
}
