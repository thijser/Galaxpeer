using System;


namespace Galaxpeer
{
	public class LocationMessage : Message
	{
		public static event MessageHandler OnReceive;


		public Vector3 Location { get; private set; }
		public Vector4 Rotation { get; private set; }
		public Vector3 Velocity { get; private set; }

		struct Packet
		{
			public char Id;
			public Vector3 Location;
			public Vector4 Rotation;
			public Vector3 Velocity;
		}

		public LocationMessage(MobileEntity mob)
		{
			Location = mob.Location;
			Rotation = mob.Rotation;
			Velocity = mob.Velocity;
		}

		public LocationMessage(byte[] bytes)
		{
			Packet packet = FromBytes<Packet> (bytes);
			Location = packet.Location;
			Rotation = packet.Rotation;
			Velocity = packet.Velocity;

			if (OnReceive != null) {
				OnReceive (this);
			}
		}

		public override byte[] Serialize()
		{
			Packet packet;
			packet.Id = 'L';
			packet.Location = Location;
			packet.Rotation = Rotation;
			packet.Velocity = Velocity;

			return ToBytes (packet);
		}
	}
}
