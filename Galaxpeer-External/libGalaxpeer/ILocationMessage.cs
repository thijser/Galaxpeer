using System;

namespace Galaxpeer
{
	public interface ILocationMessage
	{
		Guid Uuid { get; }
		long Timestamp { get; }
		Vector3 Location { get; }
		Client SourceClient { get; }
	}

	public interface IFullLocationMessage : ILocationMessage
	{
		MobileEntity.EntityType Type { get; }
		Quaternion Rotation { get; }
		Vector3 Velocity { get; }
		Guid OwnedBy { get; }
		int Health { get; }
	}
}

