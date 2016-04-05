using System;
using System.Threading;
using System.Collections.Generic;

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
			Console.WriteLine ("New Client {0}", connectionMessage.Uuid);
			ConnectionMessage = connectionMessage;
			timer = new Timer (onTimeout, null, MAX_AGE, Timeout.Infinite);
		}

		public void Tick ()
		{
			timer.Change (MAX_AGE, Timeout.Infinite);
		}

		private void onTimeout(object _)
		{
			Game.ConnectionManager.Disconnect (this);
		}

		private static Dictionary<Guid, Client> clients = new Dictionary<Guid, Client>();
		public static Client Create(ConnectionMessage message)
		{
			if (clients.ContainsKey (message.Uuid)) {
				return clients [message.Uuid];
			} else {
				var client = new Client (message);
				clients [message.Uuid] = client;
				return client;
			}
		}
	}
}
