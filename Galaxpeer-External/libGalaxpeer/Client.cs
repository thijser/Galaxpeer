
namespace Galaxpeer
{
	public class Client
	{
		public ConnectionMessage ConnectionMessage;

		private Connection connection;
		public Connection Connection
		{
			get {
				if (connection == null) {
					connection = Game.ConnectionManager.Connect (ConnectionMessage);
				}
				return connection;
			}
		}

		public Client(ConnectionMessage connectionMessage)
		{
			ConnectionMessage = connectionMessage;
		}
	}
}
