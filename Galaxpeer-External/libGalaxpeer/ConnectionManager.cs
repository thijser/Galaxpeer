﻿using System;
using System.Net;
using System.Threading;
using System.Collections.Generic;

namespace Galaxpeer
{
	// Also receives incoming connections
	public abstract class ConnectionManager
	{
		public delegate void ClientHandler (Client client);
		public static event ClientHandler OnEnterROI;
		public static event ClientHandler OnExitROI;

		private const int REQUEST_CONNECTIONS_INTERVAL = 15000;

		public ConnectionMessage LocalConnectionMessage;

		public readonly TimedCache<Guid, ConnectionMessage> ConnectionCache = new TimedCache<Guid, ConnectionMessage>();
		private readonly TimedCache<Guid, ConnectionMessage> ForwardedMessages = new TimedCache<Guid, ConnectionMessage> ();
		private readonly ConcurrentDictionary<IPEndPoint, Client> endPoints = new ConcurrentDictionary<IPEndPoint, Client>();

		public readonly Client[] ClosestClients = new Client[8];
		public readonly ConcurrentDictionary<Guid, Client> ClientsInRoi = new ConcurrentDictionary<Guid, Client> ();

		private readonly Timer requestConnectionsTimer;

		public ConnectionManager()
		{
			ConnectionCache.CacheTimeout = 30000;
			ForwardedMessages.CacheTimeout = 5000;

			Client.Clients.OnRemove += this.removeClient;

			requestConnectionsTimer = new Timer (this.RequestConnections, null, REQUEST_CONNECTIONS_INTERVAL, REQUEST_CONNECTIONS_INTERVAL); 

			ConnectionMessage.OnReceive += this.OnReceiveConnection;
			LocationMessage.OnReceive += this.OnReceiveLocation;
			RequestConnectionsMessage.OnReceive += this.OnConnectionsRequest;
		}

		public abstract Connection Connect (ConnectionMessage message);

		public void Disconnect (Client client)
		{
			Disconnect (client.Uuid);
		}

		public void Disconnect (Guid uuid)
		{
			Client.Clients.Remove (uuid);
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
				if (Position.IsEntityNearRoi(client.Player.Location, location)) {
					client.Connection.Send(message);
				}
			});
		}

		private void ForwardConnectionMessage (ConnectionMessage message)
		{
			bool forward = false;
			ForwardedMessages.Acquire (() => {
				if (!ForwardedMessages.ContainsKey(message.Uuid)) {
					ForwardedMessages.Set (message.Uuid, message);
					forward = true;
				}
			});
			if (forward) {
				ForwardMessage (message);
			}
		}

		public void ForwardMessage(ConnectionMessage message)
		{
			//if (message.Hops-- > 0) {
				foreach (var client in ClosestClients) {
					if (client != null && client != message.SourceClient) {
						if (Position.IsClientInRoi (client.Player.Location, message.Location)) {
							client.Connection.Send (message);
							//Console.WriteLine ("Forwarding message");
						}
					}
				}
			//}
		}

		public void RequestConnections(object _)
		{
			//Console.WriteLine ("Requesting connections");
			foreach (var client in ClosestClients) {
				if (client != null) {
					client.Connection.Send (new RequestConnectionsMessage (LocalPlayer.Instance.Location));
				}
			}
		}

		public bool IsClosest (Guid uuid) {
			foreach (var closest in ClosestClients) {
				if (closest != null && closest.Uuid == uuid) {
					return true;
				}
			}
			return false;
		}

		public bool IsClosest (Client client)
		{
			foreach (var closest in ClosestClients) {
				if (client == closest) {
					return true;
				}
			}
			return false;
		}

		public bool IsInRoi (Client client) {
			return ClientsInRoi.ContainsKey (client.Uuid);
		}

		public void UpdateRoiConnection(ILocationMessage message)
		{
			bool inRoi = Position.IsClientInRoi (message.Location);

			bool entered = false;
			bool exited = false;
			Client subject = null;

			ClientsInRoi.Acquire (() => {
				bool contains = ClientsInRoi.ContainsKey (message.Uuid);
				if (inRoi) {
					if (!contains) {
						if (message.GetType () == typeof(ConnectionMessage)) {
							// With a ConnectionMessage, a new Client can be created if it does not yet
							// exist.
							subject = Client.Get ((ConnectionMessage)message);
							ClientsInRoi.Set (message.Uuid, subject);
						} else {
							// With only a LocationMessage, there is insufficient information to create
							// a new Client. If the client already exists, it will be added to the 
							// list of clients in ROI, otherwise, too bad, but there is nothing to do.
							subject = Client.Clients.Get (message.Uuid);
							ClientsInRoi.Set (message.Uuid, subject);
						}
						if (Game.Config.PrintConnections) {
							Console.WriteLine ("Added client {0} to ROI", message.Uuid);
						}
						entered = true;
					}
				} else {
					if (contains) {
						subject = Client.Get (message.Uuid);
						exited = true;
						ClientsInRoi.Remove (message.Uuid);
						// Only disconnect if not in ClosestClients
						if (!IsClosest (message.Uuid)) {
							Disconnect (message.Uuid);
						}
						if (Game.Config.PrintConnections) {
							Console.WriteLine ("Removed client {0} from ROI", message.Uuid);
						}
					}
				}
			});

			// Fire events outside lock
			if (subject != null) {
				if (entered) {
					if (OnEnterROI != null) {
						OnEnterROI (subject);
					}
				} else if (exited) {
					if (OnExitROI != null) {
						OnExitROI (subject);
					}
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
						if (Game.Config.PrintConnections) {
							Console.WriteLine ("Found new client {0} in octant {1}", message.Uuid, octant);
						}
						ClosestClients [octant] = Client.Get (message);
						max_distance = distance;
					}
				}
			});
		}

		void removeClient (Guid uuid, Client client)
		{
			if (Game.Config.PrintConnections) {
				Console.WriteLine ("Dropping client {0}", uuid);
			}

			// Remove from ROI
			ClientsInRoi.Remove (uuid);

			// Remove from endpoints
			if (client.EndPoint != null) {
				endPoints.Remove (client.EndPoint);
			}

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

		private void UpdateOctant (ILocationMessage message, Client client = null)
		{
			if (message.Uuid != LocalPlayer.Instance.Uuid) {
				int octant = Position.GetOctant (LocalPlayer.Instance.Location, message.Location);
				double distance = Position.GetDistance (LocalPlayer.Instance.Location, message.Location);
				Client closest = ClosestClients [octant];

				bool newInOctant = false;

				if (closest == null) {
					if (Game.Config.PrintConnections) {
						Console.WriteLine ("New client {0} in octant {1}", message.Uuid, octant);
					}
					newInOctant = true;
				} else if (closest.Uuid != message.Uuid && distance < (Position.GetDistance (LocalPlayer.Instance.Location, closest.Player.Location) - 10)) {
					if (Game.Config.PrintConnections) {
						Console.WriteLine ("Dropping connection with {0} in octant {1} for {2}", closest.Uuid, octant, message.Uuid);
					}
					closest.Connection.Close ();
					newInOctant = true;
				}

				if (newInOctant) {
					if (client == null) {
						if (message.GetType () == typeof(ConnectionMessage)) {
							client = Client.Get ((ConnectionMessage)message);
						}
					}

					// Remove from other octants
					for (int i = 0; i < ClosestClients.Length; i++) {
						if (ClosestClients [i] == client) {
							ClosestClients [i] = null;
						}
					}
					
					ClosestClients [octant] = client;

					if (Game.Config.PrintConnections) {
						Console.WriteLine ("Connections: ");
						for (int i = 0; i < 8; i++) {
							Client c = ClosestClients [i];
							Console.WriteLine ("Octant {0}: {1}", i, c == null ? new Guid () : c.Uuid);
						}
					}
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
			if (message.Uuid != LocalPlayer.Instance.Uuid) {
				ForwardConnectionMessage (message);
				ConnectionCache.Set (message.Uuid, message);
				UpdateRoiConnection (message);
				UpdateOctant (message);
			}
		}

		protected void OnReceiveLocation(LocationMessage message)
		{
			if (message.OwnedBy != LocalPlayer.Instance.Uuid) {
				EntityManager.UpdateEntity (message);

				if ((MobileEntity.EntityType)message.Type == MobileEntity.EntityType.Player) {
					UpdateRoiConnection (message);

					Client client = ClientsInRoi.Get (message.Uuid);
					if (client != null) {
						UpdateOctant (message, client);
					}
				}
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
			for (int i = 0; i < 8; i++) {
				if (ClosestClients [i] != null && ClosestClients[i] != message.SourceClient) {
					message.SourceClient.Connection.Send (ClosestClients [i].ConnectionMessage);
				}
			}

			// Send 'random' connection
			Vector3 yourPos = message.Location;
			ConnectionMessage furthest = null;
			double distance = 0;
			ConnectionCache.ForEach ((Guid uuid, ConnectionMessage conn) => {
				double d = Position.GetDistance(yourPos, conn.Location);
				if (d >= distance) {
					furthest = conn;
					distance = d;
				}
			});

			message.SourceClient.Connection.Send (furthest);
		}
	}
}
