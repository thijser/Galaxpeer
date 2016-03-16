using System.Collections.Generic;

namespace Galaxpeer
{
	// Also receives incoming connections
	public abstract class ConnectionManager
	{
		public ConnectionMessage LocalConnectionMessage;
		public List<Connection> Connections;

		public ConnectionManager()
		{
			this.Connections = new List<Connection> ();
		}

		public abstract void Connect (ConnectionMessage message);
	}
}
