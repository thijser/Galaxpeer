using System;
using System.Net;
using System.Threading;
using System.Collections.Generic;

namespace Galaxpeer
{
	public class Client
	{
		//public const long MAX_AGE = 10000; // ms

		public static TimedCache<Guid, Client> Clients = new TimedCache<Guid, Client>();

		public delegate void CreateHandler(Client client);
		public static event CreateHandler OnCreate;

		static Client () {
			Clients.CacheTimeout = 5000;
			Clients.OnRemove += onRemove;
		}

		static void onRemove (Guid key, Client value)
		{
			value.Connection.Close ();
			value.Player.Destroy ();
		}

		public Guid Uuid;
		public ConnectionMessage ConnectionMessage;
		public Player Player;
		public Connection Connection;
		public IPEndPoint EndPoint;

		private Client (ConnectionMessage message)
		{
			Uuid = message.Uuid;
			ConnectionMessage = message;

			if (!EntityManager.Entities.ContainsKey (message.Uuid)) {
				PsycicManager.Instance.AddEntity (new Player (message));
			}
			Player = (Player) EntityManager.Get (message.Uuid);

			Connection = Game.ConnectionManager.Connect (message);
		}

		public void Update()
		{
			Game.ConnectionManager.ConnectionCache.Update (Uuid);
			Clients.Update (Uuid);
		}

		public static Client Get (Guid uuid)
		{
			return Clients.Get (uuid);
		}

		public static Client Get(ConnectionMessage message)
		{
			Client client = Clients.Get (message.Uuid);
			if (client == null) {
				bool created = false;
				Clients.Acquire (() => {
					client = Clients.Get (message.Uuid);
					if (client == null) {
						client = new Client (message);
						Clients.Set (message.Uuid, client);
						created = true;
					}
				});
				if (created && OnCreate != null) {
					OnCreate (client);
				}
			}
			return client;
		}

		/*

		private Timer timer;

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
		}*/
	}
}
