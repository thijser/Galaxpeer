using System;
using System.Collections.Generic;

namespace Galaxpeer
{
	public class EntityManager
	{
		public static ConcurrentDictionary<Guid, MobileEntity> Entities = new ConcurrentDictionary<Guid, MobileEntity>();
		private const long ASTEROID_INTERVAL = TimeSpan.TicksPerMillisecond * 500;
		private static long nextAsteroid = DateTime.UtcNow.Ticks + ASTEROID_INTERVAL;
		private static List<MobileEntity> toUpdate = new List<MobileEntity>();


		static EntityManager()
		{
			MobileEntity.OnLocationUpdate += OnLocationUpdate;
			MobileEntity.OnDestroy += OnDestroyEntity;
			PsycicManager.OnTick += GenerateAsteroids;
			PsycicManager.OnTick += SendLocationUpdates;
			DestroyMessage.OnReceive += OnDestroyMessage;
			ConnectionMessage.OnParse += OnParseConnectionMessage;
			LocationMessage.OnParse += OnParseLocationMessage;
			LocationMessage.OnReceive += OnLocationMessage;
			RequestLocationMessage.OnReceive += OnRequestLocationMessage;

			HandoverMessage.OnReceive += OnHandover;
			TakeoverMessage.OnReceive += OnTakeover;

			Client.OnCreate += OnCreateClient;
		}

		static void OnLocationMessage (LocationMessage message)
		{
			if (message.Type != MobileEntity.EntityType.Player) {
				MobileEntity entity = Get (message.Uuid);
				if (entity != null && entity.IsMine && message.OwnedBy != entity.OwnedBy) {
					// Ownership conflict! Client with lowest UUID takes ownership
					if (entity.OwnedBy.CompareTo (message.OwnedBy) < 0) {
						message.SourceClient.Connection.Send (new TakeoverMessage (entity));
					} else {
						entity.OwnedBy = message.OwnedBy;
					}
				}
			}
		}

		static void OnRequestLocationMessage (RequestLocationMessage message)
		{
			MobileEntity entity = Get (message.Uuid);
			if (entity != null) {
				message.SourceClient.Connection.Send (new LocationMessage (entity));
			}
		}

		// Send data about MobileEntities in his ROI
		static void OnParseLocationMessage (LocationMessage message)
		{
			if (message.Type == MobileEntity.EntityType.Player) {
				Client client = Client.Get (message.Uuid);
				if (client != null) {
					Entities.ForEach ((Guid uuid, MobileEntity entity) => {
						if (entity.IsMine) {
							if (Position.IsEntityInRoi (entity.Location, message.Location)) {
								if (!Position.IsEntityInRoi (entity.Location, client.Player.Location)) {
									client.Connection.Send (new LocationMessage (entity));
									Console.WriteLine ("Sent location of {0} to {1}", entity.Uuid, client.Uuid);
								}
							}
						}
					});
				}
			}
		}

		static void OnTakeover (TakeoverMessage message)
		{
			MobileEntity entity = Get (message.ObjectUuid);
			if (entity != null) {
				entity.Takeover (message.OwnerUuid);
			}
		}

		static void OnHandover (HandoverMessage message)
		{
			MobileEntity entity = Get (message.ObjectUuid);
			if (entity != null) {
				entity.OwnedBy = LocalPlayer.Instance.Uuid;
				message.SourceClient.Connection.Send (new TakeoverMessage (entity));
				//Game.ConnectionManager.SendInRoi (new TakeoverMessage (entity), entity.Location);
			} else {
				Console.WriteLine ("Unknown entity {0}", message.ObjectUuid);
				message.SourceClient.Connection.Send (new RequestLocationMessage (message.ObjectUuid));
			}
		}

		static void OnParseConnectionMessage (ConnectionMessage message)
		{
			if (!Entities.ContainsKey (message.Uuid) && Position.IsClientInRoi (message.Location)) {
				Entities.Add (message.Uuid, new Player (message));
			}
		}

		static void OnCreateClient (Client client)
		{
			Vector3 l = client.Player.Location;
			Console.WriteLine ("New client {0} at {1} {2} {3}", client.Uuid, l.X, l.Y, l.Z);
			Entities.ForEach ((Guid uuid, MobileEntity entity) => {
				if (Position.IsEntityInRoi(entity.Location, client.Player.Location)) {
					client.Connection.Send (new LocationMessage(entity));
					Console.WriteLine("Sent location of {0} to new client {1}", entity.Uuid, client.Uuid);
				}
			});
		}

		static void OnDestroyEntity (MobileEntity entity, bool owned)
		{
			if (owned) {
				Game.ConnectionManager.SendInRoi (new DestroyMessage(entity), entity.Location);
			}
		}

		private static void OnLocationUpdate(MobileEntity entity, bool owned)
		{
			if (owned) {
				lock (toUpdate) {
					toUpdate.Add (entity);
				}
			}
		}

		private static void OnDestroyMessage(DestroyMessage message)
		{
			MobileEntity entity = Get (message.Uuid);
			if (entity != null) {
				entity.Destroy ();
			}
		}

		public static void Add (Guid uuid, MobileEntity value)
		{
			Entities.Set (uuid, value);
		}

		public static MobileEntity Get (Guid uuid)
		{
			return Entities.Get (uuid);
		}

		public static void Remove (Guid uuid)
		{
			Entities.Remove (uuid);
		}

		public static void Remove (MobileEntity entity)
		{
			Remove (entity.Uuid);
		}

		public static void UpdateEntity(LocationMessage message)
		{
			MobileEntity entity = Get (message.Uuid);
			if (entity == null) {
				if (Position.IsEntityInRoi(LocalPlayer.Instance.Location, message.Location)) {
					CreateEntity (message);
				}
			} else {
				entity.Update (message);
			}
		}

		public static void CreateEntity(LocationMessage message)
		{
			MobileEntity entity = null;
			MobileEntity.EntityType type = (MobileEntity.EntityType)message.Type;
			switch (type) {
			case MobileEntity.EntityType.Asteroid:
				entity = new Asteroid (message);
				break;
			case MobileEntity.EntityType.Player:
				entity = new Player (message);
				break;
			case MobileEntity.EntityType.Rocket:
				entity = new Rocket (message);
				break;
			default:
				return;
			}
			PsycicManager.Instance.AddEntity (entity);
			Entities.Set(message.Uuid, entity);
		}

		static void SendLocationUpdates(long time)
		{
			lock (toUpdate) {
				foreach (var entity in toUpdate) {
					var msg = new LocationMessage (entity);
					Game.ConnectionManager.SendInRoi (msg, entity.Location);
				}
				toUpdate.Clear ();
			}
		}

		private static void GenerateAsteroids(long time)
		{
			if (time >= nextAsteroid) {
				nextAsteroid = time + ASTEROID_INTERVAL;

				// Only generate asteroid if it is outside ROI of other players
				Asteroid a = new Asteroid ();

				if (!Position.IsInAnyRoi (a.Location)) {
					PsycicManager.Instance.AddEntity (a);
				}
			}
		}
	}
}
