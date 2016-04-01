using System;
using System.Collections.Generic;

namespace Galaxpeer
{
	public class EntityManager
	{

		public static Dictionary<Guid, MobileEntity> Entities = new Dictionary<Guid, MobileEntity>();


		static EntityManager()
		{
			MobileEntity.OnLocationUpdate += OnLocationUpdate;
		}

		private static void OnLocationUpdate(MobileEntity entity, bool owned)
		{
			if (owned) {
				var msg = new LocationMessage (entity);
				foreach (var item in Game.ConnectionManager.ClientsInRoi) {
					bool inRoi = Position.IsInRoi (item.Value.Player.Location, entity.Location);
					if (inRoi) {
						item.Value.Connection.Send (msg);
					}
				}
			}
		}

		public static MobileEntity Get (Guid uuid) {
			MobileEntity entity;
			Entities.TryGetValue (uuid, out entity);
			return entity;
		}

		public static void UpdateEntity(LocationMessage message)
		{
			MobileEntity entity = Get (message.Uuid);
			if (entity == null) {
				CreateEntity (message);
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
			Entities [message.Uuid] = entity;
		}
	}
}
