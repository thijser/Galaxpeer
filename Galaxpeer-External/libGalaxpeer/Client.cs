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
			Clients.CacheTimeout = 10000;
			Clients.CacheInterval = 3000;
			Clients.OnRemove += onRemove;
			Clients.OnInterval += SendHeartbeat;

			HeartbeatMessage.OnReceive += OnHeartbeat;
		}

		static void OnHeartbeat (HeartbeatMessage message)
		{
			message.SourceClient.Connection.Send (Game.ConnectionManager.LocalConnectionMessage);
		}

		static void onRemove (Guid key, Client value)
		{
			value.Connection.Close ();
			value.Player.Destroy ();
		}

		static void SendHeartbeat (Guid uuid, Client client)
		{
			client.Connection.Send (new HeartbeatMessage ());
		}

		public Guid Uuid;
		public ConnectionMessage ConnectionMessage;
		public Player Player;
		public Connection Connection;
		public IPEndPoint EndPoint;
		private long lastUpdate;

		private Client (ConnectionMessage message)
		{
			Uuid = message.Uuid;
			ConnectionMessage = message;

			EntityManager.Entities.Acquire (() => {
				if (!EntityManager.Entities.ContainsKey (message.Uuid)) {
					Player = new Player (message);
					PsycicManager.Instance.AddEntity (Player);
					EntityManager.Entities.Set (message.Uuid, Player);
				} else {
					Player = (Player) EntityManager.Entities.Get (message.Uuid);
				}
			});

			Connection = Game.ConnectionManager.Connect (message);

			lastUpdate = DateTime.UtcNow.Ticks;
		}

		public void Update()
		{
			if (lastUpdate < DateTime.UtcNow.Ticks - 1000) {
				Game.ConnectionManager.ConnectionCache.Update (Uuid);
				Clients.Update (Uuid);
				lastUpdate = DateTime.UtcNow.Ticks;
			}
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
	}
}
