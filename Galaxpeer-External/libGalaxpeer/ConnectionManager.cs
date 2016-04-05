using System;
using System.Net;
using System.Collections.Generic;

namespace Galaxpeer
{
	// Also receives incoming connections
	public abstract class ConnectionManager
	{
		public ConnectionMessage LocalConnectionMessage;

		public Dictionary<Guid, ConnectionMessage> ConnectionCache = new Dictionary<Guid, ConnectionMessage>();
		private readonly Dictionary<IPEndPoint, Client> endPoints = new Dictionary<IPEndPoint, Client>();
		public readonly Client[] ClosestClients = new Client[8];
		public Dictionary<Guid, Client> ClientsInRoi = new Dictionary<Guid, Client> ();

		private readonly List<Client> staleClients = new List<Client>();

		private readonly List<ConnectionMessage> newConnections = new List<ConnectionMessage>();
		private readonly List<ConnectionMessage> staleConnections = new List<ConnectionMessage> ();

		public ConnectionManager()
		{
			ConnectionMessage.OnReceive += this.OnReceiveConnection;
			LocationMessage.OnReceive += this.OnReceiveLocation;
			RequestConnectionsMessage.OnReceive += this.OnConnectionsRequest;
			PsycicManager.OnTick += this.OnTick;
		}

		public abstract Connection Connect (ConnectionMessage message);

		public void Disconnect(Client client)
		{
			staleClients.Add (client);
		}

		// TODO: remove client from endPoints
		private void disconnectStaleClients()
		{
			while (staleClients.Count > 0) {
				var client = staleClients [0];
				Console.WriteLine ("Dropping client {0}", client.Uuid);
				client.Connection.Close ();
				ConnectionCache.Remove (client.Uuid);
				ClientsInRoi.Remove (client.Uuid);

				for (int i = 0; i < ClosestClients.Length; i++) {
					if (ClosestClients[i] == client) {
						ClosestClients [i] = null;
						FindClosestClient (i);
					}
				}
				staleClients.Remove (client);
			}
		}

		public void AddByEndPoint(IPEndPoint endPoint, ConnectionMessage connection)
		{
			endPoints [endPoint] = new Client (connection);
		}

		public Client GetByEndPoint(IPEndPoint endPoint)
		{
			Client client;
			endPoints.TryGetValue (endPoint, out client);
			return client;
		}

		protected void cleanConnectionCache()
		{
			List<Guid> toRemove = new List<Guid> ();
			foreach (var item in ConnectionCache) {
				if (item.Value.Timestamp >= DateTime.UtcNow.Ticks - ConnectionMessage.MAX_AGE) {
					Console.WriteLine ("Removing connection {0}", item.Key);
					toRemove.Add (item.Key);
				}
			}

			foreach (var item in toRemove) {
				ConnectionCache.Remove (item);
			}
		}

		public void ForwardMessage(Message message)
		{
			if (--message.Hops > 0) {
				foreach (var client in ClosestClients) {
					if (client != null && client != message.SourceClient) {
						client.Connection.Send (message);
					}
				}
			}
		}

		public void UpdateRoiConnection(Client client, Vector3 location)
		{
			if (Position.IsInRoi (LocalPlayer.Instance.Location, location)) {
				if (!ClientsInRoi.ContainsKey (client.Uuid)) {
					ClientsInRoi.Add (client.Uuid, client);
				}
			} else {
				if (ClientsInRoi.ContainsKey (client.Uuid)) {
					ClientsInRoi.Remove (client.Uuid);
					client.Connection.Close ();
				}
			}
		}

		public void FindClosestClient(int octant)
		{
			double max_distance = double.MaxValue;
			foreach (var message in ConnectionCache.Values) {
				int messageOctant = Position.GetOctant (LocalPlayer.Instance.Location, message.Location);
				if (messageOctant == octant) {
					double distance = Position.GetDistance (LocalPlayer.Instance.Location, message.Location);
					if (distance <= max_distance) {
						Console.WriteLine ("Found new client {0}", message.Uuid);
						ClosestClients [octant] = new Client (message); // TODO: prevent creating duplicate clients
						max_distance = distance;
					}
				}
			}
		}

		void OnTick(long time)
		{
			while (newConnections.Count > 0) {
				var message = newConnections [0];
				ConnectionCache [message.Uuid] = message;
				newConnections.Remove (message);
			}
			disconnectStaleClients ();
		}

		/* Handle a received connection message.
		 * 
		 * Add to connection list
		 * Check if it should replace an existing connection
		 */
		protected void OnReceiveConnection(ConnectionMessage message)
		{
			if (message.Uuid != LocalConnectionMessage.Uuid) {
				long age = DateTime.UtcNow.Ticks - message.Timestamp;
				if (age <= ConnectionMessage.MAX_AGE) {
					//ConnectionCache [message.Uuid] = message;
					newConnections.Add(message);

					int octant = Position.GetOctant (LocalPlayer.Instance.Location, message.Location);
					double distance = Position.GetDistance (LocalPlayer.Instance.Location, message.Location);
					Client closest = ClosestClients [octant];

					if (closest == null) {
						Console.WriteLine ("New client {0} in octant {1}", message.Uuid, octant);
						ClosestClients [octant] = new Client (message);
						message.SourceClient.Connection.Send (new RequestConnectionsMessage (LocalPlayer.Instance.Location));
					} else if (distance < (Position.GetDistance (LocalPlayer.Instance.Location, closest.Player.Location) - 5)) {
						Console.WriteLine ("Dropping connection with {0} in octant {1} for {2} ({3})", closest.ConnectionMessage.Uuid, octant, message.Uuid, message.SourceClient.Player.Uuid);
						closest.Connection.Close ();
						ClosestClients [octant] = new Client (message);
						message.SourceClient.Connection.Send (new RequestConnectionsMessage (LocalPlayer.Instance.Location));
					}

					// Check if this player is in ROI
					UpdateRoiConnection (message.SourceClient, message.Location);
				}
			}
		}

		protected void OnReceiveLocation(LocationMessage message)
		{
			EntityManager.UpdateEntity (message);

			if ((MobileEntity.EntityType)message.Type == MobileEntity.EntityType.Player) {
				UpdateRoiConnection (message.SourceClient, message.Location);
			}
		}

		/* Send data of clients closest to specified location.
		 * 
		 * Loop over connection cache, removing any messages that are too old.
		 * Keep track of closest clients in each of the octants.
		 * If a newer location of client is known, use that.
		 */
		protected void OnConnectionsRequest(RequestConnectionsMessage message)
		{
			cleanConnectionCache ();
			ConnectionMessage[] connections = new ConnectionMessage[8];
			foreach (var item in ConnectionCache) {
				ConnectionMessage conn = item.Value;

				if (conn.Uuid != message.SourceClient.ConnectionMessage.Uuid) {
					Guid id = item.Key;
					MobileEntity entity = EntityManager.Get (id);

					// Update location to newer value
					if (entity != null
					   && entity.LastUpdate > conn.Timestamp) {
						conn.Location = entity.Location;
					}

					int octant = Position.GetOctant (message.Location, conn.Location);
					double distance = Position.GetOctant (message.Location, conn.Location);
					ConnectionMessage closest = connections [octant];

					if (closest == null || distance < (Position.GetDistance (message.Location, closest.Location) - 5)) {
						connections [octant] = conn;
					}
				}
			}

			// Return connections
			foreach (var item in connections) {
				if (item != null) {
					message.SourceClient.Connection.Send (item);
				}
			}
		}
	}
}
