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
			endPoints.Add (endPoint, new Client(connection));
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
			if (message.Timestamp >= DateTime.UtcNow.Ticks - ConnectionMessage.MAX_AGE) {
				ConnectionCache [message.Uuid] = message;

				int octant = Position.GetOctant (LocalPlayer.Instance.Location, message.Location);
				double distance = Position.GetDistance (LocalPlayer.Instance.Location, message.Location);
				Client closest = closestClients [octant];

				if (closest == null) {
					closestClients [octant] = client;
				} else if (distance < Position.GetDistance (LocalPlayer.Instance.Location, closest.Player.Location)) {
					closest.Connection.Close ();
					closestClients [octant] = client;
				}
			}
		}

		protected void OnReceiveLocation(Client client, LocationMessage message)
		{
			if (message.Type == (byte)MobileEntity.EntityType.Player) {

			}
		}

		/* Send data of clients closest to specified location.
		 * 
		 * Loop over connection cache, removing any messages that are too old.
		 * Keep track of closest clients in each of the octants.
		 */
		protected void OnConnectionsRequest(Client client, RequestConnectionsMessage message)
		{
			cleanConnectionCache ();
			ConnectionMessage[] connections = new ConnectionMessage[8];
			foreach (var item in ConnectionCache.Values) {
				
			}
		}
	}
}
