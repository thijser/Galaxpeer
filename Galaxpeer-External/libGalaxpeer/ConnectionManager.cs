using System.Collections.Generic;

namespace Galaxpeer
{
	// Also receives incoming connections
	public class ConnectionManager
	{
		public ConnectionMessage LocalConnectionMessage;
		public List<Connection> Connections;
	}
}
