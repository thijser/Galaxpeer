using System;
using System.Net;
using System.Collections.Generic;

namespace Galaxpeer
{
	// Also receives incoming connections
	public abstract class ConnectionManager
	{
		public ConnectionMessage LocalConnectionMessage;

		public readonly TimedCache<Guid, ConnectionMessage> ConnectionCache = new TimedCache<Guid, ConnectionMessage>();
		private readonly ConcurrentDictionary<IPEndPoint, Client> endPoints = new ConcurrentDictionary<IPEndPoint, Client>();

		public readonly Client[] ClosestClients = new Client[8];
		public readonly ConcurrentDictionary<Guid, Client> ClientsInRoi = new ConcurrentDictionary<Guid, Client> ();


		//public Dictionary<Guid, ConnectionMessage> ConnectionCache = new Dictionary<Guid, ConnectionMessage>();
		//private readonly Dictionary<IPEndPoint, Client> endPoints = new Dictionary<IPEndPoint, Client>();
		//public readonly Client[] ClosestClients = new Client[8];
		//public Dictionary<Guid, Client> ClientsInRoi = new Dictionary<Guid, Client> ();

		//private readonly List<Client> staleClients = new List<Client>();

		//private readonly List<ConnectionMessage> newConnections = new List<ConnectionMessage>();
		//private readonly List<ConnectionMessage> staleConnections = new List<ConnectionMessage> ();

		public ConnectionManager()
		{
			ConnectionCache.CacheTimeout = 30000;
			Client.Clients.OnRemove += this.removeClient;

			ConnectionMessage.OnReceive += this.OnReceiveConnection;
			LocationMessage.OnReceive += this.OnReceiveLocation;
			RequestConnectionsMessage.OnReceive += this.OnConnectionsRequest;
		}

		public abstract Connection Connect (ConnectionMessage message);

		public void Disconnect(Client client)
		{
			Client.Clients.Remove (client.Uuid);
		}

		public void AddByEndPoint(IPEndPoint endPoint, ConnectionMessage connection)
		{
			Client client = Client.Get (connection);
			client.EndPoint = endPoint;
			endPoints.Set(endPoint, client);
		}

		public Client GetByEndPoint(IPEndPoint endPoint)
		{
			return endPoints.Get (endPoint);
		}

		public void SendInRoi(Message message)
		{
			SendInRoi(message, LocalPlayer.Instance.Location);
		}

		public void SendInRoi(Message message, Vector3 location)
		{
			ClientsInRoi.ForEach ((Guid uuid, Client client) => {
				if (Position.IsEntityInRoi(client.Player.Location, location)) {
					client.Connection.Send(message);
				}
			});
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
			if (Position.IsClientInRoi (location)) {
				if (!ClientsInRoi.ContainsKey (client.Uuid)) {
					ClientsInRoi.Set (client.Uuid, client);
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
			ConnectionCache.ForEach ((Guid key, ConnectionMessage message) => {
				int messageOctant = Position.GetOctant (LocalPlayer.Instance.Location, message.Location);
				if (messageOctant == octant) {
					double distance = Position.GetDistance (LocalPlayer.Instance.Location, message.Location);
					if (distance <= max_distance) {
						Console.WriteLine ("Found new client {0}", message.Uuid);
						ClosestClients [octant] = Client.Get (message);
						max_distance = distance;
					}
				}
			});
		}

		void removeClient (Guid uuid, Client client)
		{
			Console.WriteLine ("Dropping client {0}", uuid);

			// Remove from ROI
			ClientsInRoi.Remove (uuid);

			// Remove from endpoints
			endPoints.Remove (client.EndPoint);

			// Remove from connection cache
			ConnectionCache.Remove (uuid);

			// Remove from closest clients
			for (int i = 0; i < ClosestClients.Length; i++) {
				if (ClosestClients[i] == client) {
					ClosestClients[i] = null;
					FindClosestClient(i);
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
			if (message.Uuid != LocalConnectionMessage.Uuid && message.SourceClient != null) {
				long age = DateTime.UtcNow.Ticks - message.Timestamp;
				if (age <= ConnectionMessage.MAX_AGE) {
					ConnectionCache.Set (message.Uuid, message);

					int octant = Position.GetOctant (LocalPlayer.Instance.Location, message.Location);
					double distance = Position.GetDistance (LocalPlayer.Instance.Location, message.Location);
					Client closest = ClosestClients [octant];

					if (closest == null) {
						Console.WriteLine ("New client {0} in octant {1}", message.Uuid, octant);
						ClosestClients [octant] = Client.Get (message);
						message.SourceClient.Connection.Send (new RequestConnectionsMessage (LocalPlayer.Instance.Location));
					} else if (distance < (Position.GetDistance (LocalPlayer.Instance.Location, closest.Player.Location) - 5)) {
						Console.WriteLine ("Dropping connection with {0} in octant {1} for {2} ({3})", closest.Uuid, octant, message.Uuid, message.SourceClient.Player.Uuid);
						closest.Connection.Close ();
						ClosestClients [octant] = Client.Get (message);
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
			ConnectionMessage[] connections = new ConnectionMessage[8];
			ConnectionCache.ForEach ((Guid id, ConnectionMessage conn) => {
				if (id != message.SourceClient.Uuid) {
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
			});

			// Return connections
			foreach (var item in connections) {
				if (item != null) {
					message.SourceClient.Connection.Send (item);
				}
			}
		}
	}
}
