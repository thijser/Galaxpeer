using System;
using System.Net;
using System.Collections.Generic;
//using System.Linq;

namespace Galaxpeer
{
	// Also receives incoming connections
	public abstract class ConnectionManager
	{
		public ConnectionMessage LocalConnectionMessage;

		public Dictionary<Guid, ConnectionMessage> ConnectionCache = new Dictionary<Guid, ConnectionMessage>();
		private readonly Dictionary<IPEndPoint, Client> endPoints = new Dictionary<IPEndPoint, Client>();
		private readonly Client[] closestClients = new Client[8];

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
//			var toRemove = ConnectionCache.Where (v => v.Value.Timestamp >= DateTime.UtcNow.Ticks - ConnectionMessage.MAX_AGE).ToList();
//			foreach (var item in toRemove) {
//				ConnectionCache.Remove (item.Key);
//			}
		}

		/* Handle a received connection message.
		 * 
		 * Add to connection list
		 * Check if it should replace an existing connection
		 */
		protected void OnReceiveConnection(Client client, ConnectionMessage message)
		{
			if (message.Uuid != LocalConnectionMessage.Uuid) {
				long age = DateTime.UtcNow.Ticks - message.Timestamp;
				if (age <= ConnectionMessage.MAX_AGE) {
					ConnectionCache [message.Uuid] = message;

					int octant = Position.GetOctant (LocalPlayer.Instance.Location, message.Location);
					double distance = Position.GetDistance (LocalPlayer.Instance.Location, message.Location);
					Client closest = closestClients [octant];

					if (closest == null) {
						Console.WriteLine ("New client {0} in octant {1}", message.Uuid, octant);
						closestClients [octant] = new Client (message);
						client.Connection.Send (new RequestConnectionsMessage (LocalPlayer.Instance.Location));
					} else if (distance < (Position.GetDistance (LocalPlayer.Instance.Location, closest.Player.Location) - 5)) {
						Console.WriteLine ("Dropping connection with {0} in octant {1} for {2} ({3})", closest.ConnectionMessage.Uuid, octant, message.Uuid, client.Player.Uuid);
						closest.Connection.Close ();
						closestClients [octant] = new Client (message);
						client.Connection.Send (new RequestConnectionsMessage (LocalPlayer.Instance.Location));
					}
				}
			}
		}

		protected void OnReceiveLocation(Client client, LocationMessage message)
		{
			EntityManager.UpdateEntity (message);
		}

		/* Send data of clients closest to specified location.
		 * 
		 * Loop over connection cache, removing any messages that are too old.
		 * Keep track of closest clients in each of the octants.
		 * If a newer location of client is known, use that.
		 */
		protected void OnConnectionsRequest(Client client, RequestConnectionsMessage message)
		{
			cleanConnectionCache ();
			ConnectionMessage[] connections = new ConnectionMessage[8];
			foreach (var item in ConnectionCache) {
				ConnectionMessage conn = item.Value;

				if (conn.Uuid != client.ConnectionMessage.Uuid) {
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
					Console.WriteLine (item.Uuid);
					client.Connection.Send (item);
				}
			}
		}
	}
}
