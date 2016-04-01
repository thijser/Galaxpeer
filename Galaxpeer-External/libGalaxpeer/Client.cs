using System;

namespace Galaxpeer
{
	public class Client
	{
		public const long MAX_AGE = 1 * TimeSpan.TicksPerMinute;
		
		public long LastActivity;
		public ConnectionMessage ConnectionMessage;

		public bool IsAlive
		{
			get {
				return DateTime.UtcNow.Ticks - LastActivity <= MAX_AGE;
			}
		}

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
