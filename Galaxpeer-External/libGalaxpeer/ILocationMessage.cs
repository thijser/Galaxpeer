using System;

namespace Galaxpeer
{
	public interface ILocationMessage
	{
		Guid Uuid { get; }
		Vector3 Location { get; }
		Client SourceClient { get; }
	}
}

