using System;
using System.Threading;

namespace Galaxpeer
{
	public class Client
	{
		public const long MAX_AGE = 10000; // ms

		public ConnectionMessage ConnectionMessage;

		private Timer timer;

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
			timer = new Timer (onTimeout, null, MAX_AGE, Timeout.Infinite);
		}

		public void Tick ()
		{
			timer.Change (MAX_AGE, 0);
		}

		private void onTimeout(object _)
		{
			Game.ConnectionManager.Disconnect (this);
		}
	}
}
