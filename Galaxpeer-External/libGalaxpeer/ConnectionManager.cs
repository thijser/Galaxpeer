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
		private readonly Client[] closestClients = new Client[8];

		public ConnectionManager()
		{
			ConnectionMessage.OnReceive += this.OnReceiveConnection;
			LocationMessage.OnReceive += this.OnReceiveLocation;
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

		/* Handle a received connection message.
		 * 
		 * Add to connection list
		 * Check if it should replace an existing connection
		 */
		protected void OnReceiveConnection(Client client, ConnectionMessage message)
		{
			ConnectionCache [message.Uuid] = message;

			int octant = Position.GetOctant (message.Location);
			double distance = Position.GetDistance (message.Location);
			Client closest = closestClients [octant];

			if (closest == null) {
				closestClients [octant] = client;
			} else if (distance < Position.GetDistance (closest.Player.Location)) {
				closest.Connection.Close ();
				closestClients [octant] = client;
			}
		}

		protected void OnReceiveLocation(Client client, LocationMessage message)
		{
			if (message.Type == (byte)MobileEntity.EntityType.Player) {

			}
		}
	}
}
