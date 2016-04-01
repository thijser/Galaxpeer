using System;

namespace Galaxpeer
{
	public class Client
	{
		public ConnectionMessage ConnectionMessage;

		public Guid Uuid
		{
			get {
				return ConnectionMessage.Uuid;
			}
		}

		public Player Player
		{
			get {
				return (Player) EntityManager.Get (ConnectionMessage.Uuid);
			}
		}

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
