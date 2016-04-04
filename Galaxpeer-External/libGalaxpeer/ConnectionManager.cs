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

		public ConnectionManager()
		{
			ConnectionMessage.OnReceive += this.OnReceiveConnection;
			LocationMessage.OnReceive += this.OnReceiveLocation;
			RequestConnectionsMessage.OnReceive += this.OnConnectionsRequest;
		}

		public abstract Connection Connect (ConnectionMessage message);

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

		public void cleanClientsInRoi()
		{
			List<Guid> toRemove = new List<Guid> ();
			foreach (var item in ClientsInRoi) {
				if (!item.Value.IsAlive) {
					toRemove.Add (item.Key);
				}
			}

			foreach (var item in toRemove) {
				ClientsInRoi.Remove (item);
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
					ConnectionCache [message.Uuid] = message;

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
