using System.Net.Sockets;

namespace Galaxpeer
{
	public abstract class Connection
	{
		public Connection(ConnectionMessage message)
		{

		}

		public abstract void Send (Message message);
		public abstract void Close ();
	}
}
