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
			ConnectionMessage.OnParse += OnConnectionMessage;

			Client.OnCreate += OnCreateClient;
		}

		static void OnConnectionMessage (ConnectionMessage message)
		{
			if (!Entities.ContainsKey (message.Uuid) && Position.IsClientInRoi (message.Location)) {
				Entities.Add (message.Uuid, new Player (message));
			}
		}

		static void OnCreateClient (Client client)
		{
			// TODO: Send mobile entities
			Console.WriteLine("Created client {0}", client.Uuid);
		}

		static void OnDestroyEntity (MobileEntity entity, bool owned)
		{
			if (owned) {
				var msg = new DestroyMessage (entity);
				Game.ConnectionManager.SendInRoi (msg, entity.Location);
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
