using System;
using System.Net;
using System.Collections.Generic;

namespace Galaxpeer
{
	// Also receives incoming connections
	public abstract class ConnectionManager
	{
		public ConnectionMessage LocalConnectionMessage;

		public Dictionary<Guid, ConnectionMessage> ConnectionMessages = new Dictionary<Guid, ConnectionMessage>();
		private readonly Dictionary<IPEndPoint, Client> endPoints = new Dictionary<IPEndPoint, Client>();

		public ConnectionManager()
		{
			ConnectionMessage.OnReceive += this.OnReceiveConnection;
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
		 */
		protected void OnReceiveConnection(Client client, ConnectionMessage message)
		{
			ConnectionMessages [message.Uuid] = message;
		}
	}
}
