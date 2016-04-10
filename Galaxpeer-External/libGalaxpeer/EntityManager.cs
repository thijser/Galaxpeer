using System;
using System.Threading;
using System.Collections.Generic;

namespace Galaxpeer
{
	public class EntityManager
	{
		public static ConcurrentDictionary<Guid, MobileEntity> Entities = new ConcurrentDictionary<Guid, MobileEntity>();
		private const long ASTEROID_INTERVAL = 3000;
		private const int MAX_ENTITIES = 150;
		private static List<MobileEntity> toUpdate = new List<MobileEntity>();
		private static Timer asteroidTimer;

		static EntityManager()
		{
			asteroidTimer = new Timer (GenerateAsteroids, null, ASTEROID_INTERVAL, ASTEROID_INTERVAL);

			MobileEntity.OnLocationUpdate += OnLocationUpdate;
			MobileEntity.OnPeriodicUpdate += OnPeriodicEntityUpdate;
			MobileEntity.OnTimeout += OnEntityTimeout;
			MobileEntity.OnDestroy += OnDestroyEntity;
			PsycicManager.OnTick += SendLocationUpdates;
			DestroyMessage.OnReceive += OnDestroyMessage;
			ConnectionMessage.OnParse += OnParseConnectionMessage;
			RequestLocationMessage.OnReceive += OnRequestLocationMessage;

			HandoverMessage.OnReceive += OnHandover;
			TakeoverMessage.OnReceive += OnTakeover;

			ConnectionManager.OnEnterROI += ForwardAllLocations;
		}

		static void OnPeriodicEntityUpdate (MobileEntity entity, bool owned)
		{
			if (owned) {
				Game.ConnectionManager.SendInRoi (new LocationMessage (entity), entity.Location);
			}
		}

		static void OnEntityTimeout (MobileEntity entity, bool owned)
		{
			if (!owned) {
				if (entity.Type != MobileEntity.EntityType.Player) {
					if (Game.Config.PrintEntities) {
						Console.WriteLine ("Entity {0} of {1} timed out", entity.Uuid, entity.OwnedBy);
					}
					if (Position.IsEntityInRoi (LocalPlayer.Instance.Location, entity.Location)) {
						Client closest;
						if (!Position.ClosestClient (entity.Location, out closest)) {
							entity.Takeover ();
						}
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

		static void OnTakeover (TakeoverMessage message)
		{
			if (Game.Config.PrintEntities) {
				Console.WriteLine ("{0} takes over {1}", message.OwnerUuid, message.ObjectUuid);
			}
			MobileEntity entity = Get (message.ObjectUuid);
			if (entity != null) {
				entity.OnTakeover (message.OwnerUuid);
			}
		}

		static void OnHandover (HandoverMessage message)
		{
			if (Game.Config.PrintEntities) {
				Console.WriteLine ("{0} hands over {1}", message.SourceClient.Uuid, message.Uuid);
			}
			MobileEntity entity = UpdateEntity (message);
			if (entity != null) {
				entity.Takeover (message.SourceClient);
			}
		}

		static void OnParseConnectionMessage (ConnectionMessage message)
		{
			if (Position.IsClientInRoi (message.Location)) {
				Entities.Acquire (() => {
					if (!Entities.ContainsKey (message.Uuid)) {
						Player player = new Player (message);
						Entities.Set (message.Uuid, player);
						PsycicManager.Instance.AddEntity (player);
					}
				});
			}
		}

		static void ForwardAllLocations (Client client)
		{
			Vector3 l = client.Player.Location;
			//Console.WriteLine ("New client {0} at {1} {2} {3}", client.Uuid, l.X, l.Y, l.Z);
			Entities.ForEach ((Guid uuid, MobileEntity entity) => {
				if (Position.IsEntityNearRoi(entity.Location, client.Player.Location)) {
					client.Connection.Send (new LocationMessage(entity));
					//Console.WriteLine("Sent location of {0} to new client {1}", entity.Uuid, client.Uuid);
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

		public static MobileEntity UpdateEntity(IFullLocationMessage message)
		{
			MobileEntity entity = null;
			bool doUpdate = true;
			Entities.Acquire (() => {
				entity = Get (message.Uuid);
				if (entity == null) {
					if (Position.IsEntityInRoi(LocalPlayer.Instance.Location, message.Location)) {
						entity = CreateEntity (message);
					}
					doUpdate = false;
				}
			});

			if (doUpdate) {
				entity.Update (message);
			}
			return entity;
		}

		public static MobileEntity CreateEntity(IFullLocationMessage message)
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
				return null;
			}
			PsycicManager.Instance.AddEntity (entity);
			Entities.Set(message.Uuid, entity);
			return entity;
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

		private static void GenerateAsteroids(object _)
		{
			// Only generate asteroid if space is not already filled with entities
			if (Entities.Count <= MAX_ENTITIES) {
				// Only generate asteroid if it is outside ROI of other players
				Asteroid a = new Asteroid ();

				if (!Position.IsInAnyRoi (a.Location)) {
					PsycicManager.Instance.AddEntity (a);
				}
			}
		}
	}
}
