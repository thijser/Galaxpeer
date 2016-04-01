using System.Collections.Generic;
using System;

namespace Galaxpeer
{
	class PsycicManager{
		List<MobileEntity> objects;
		static PsycicManager instance;
		private static object syncRoot = new Object();
		public List<MobileEntity> Destoyed;
		public static PsycicManager Instance
		{
			get 
			{
				if (instance == null) 
				{
					lock (syncRoot) 
					{
						if (instance == null) 
							instance = new PsycicManager();
					}
				}

				return instance;
			}
		}

		public void tick(){
			foreach (MobileEntity moe in objects) {
				moe.LocationUpdate (1);

				foreach (MobileEntity moe2 in objects){
					if (moe.CheckCollision (moe2)) {
						moe.collide (moe2);
					}	
				}
			}

			Cleanup ();
		}

		private void Cleanup(){
			while (Destoyed.Count != 0) {
				var entity = Destoyed [0];
				objects.Remove (entity);
				Destoyed.Remove (entity);
			}
		}
	}
}