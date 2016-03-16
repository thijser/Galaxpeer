using System.Collections.Generic;

namespace Galaxpeer
{
	// Also receives incoming connections
	public class ConnectionManager
	{
		ConnectionMessage LocalConnection;
		public List<ConnectionMessage> Connections;
	}
}
