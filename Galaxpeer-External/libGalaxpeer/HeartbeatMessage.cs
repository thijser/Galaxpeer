﻿using System;
using System.Runtime.InteropServices;

namespace Galaxpeer
{
	class HeartbeatMessage : TMessage<HeartbeatMessage>
	{
		public override char Id { get { return 'B'; } }
		[StructLayout(LayoutKind.Sequential, Pack=1, CharSet=CharSet.Unicode)]
		struct Packet
		{
			public char Id;
			public long Timestamp;
		}

		public HeartbeatMessage() {}

		public HeartbeatMessage(byte[] bytes)
		{
			Packet packet = FromBytes<Packet> (bytes);
			Timestamp = packet.Timestamp;
		}

		public override byte[] Serialize()
		{
			Packet packet;
			packet.Id = Id;
			packet.Timestamp = Timestamp;

			return ToBytes (packet);
		}
	}
}

