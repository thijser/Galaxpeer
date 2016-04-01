﻿using System;
using System.Collections.Generic;

namespace Galaxpeer
{
	public class EntityManager
	{
		public static Dictionary<Guid, MobileEntity> Entities = new Dictionary<Guid, MobileEntity>();

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
