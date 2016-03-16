using System.Collections.Generic;

namespace Galaxpeer
{
	class PsycicManager{
		List<MobileEntity> objects;

		public void tick(){
			foreach (MobileEntity moe in objects) {
				moe.LocationUpdate (1);

				foreach (MobileEntity moe2 in objects){
					if (moe.CheckCollision (moe2)) {
						moe.collide (moe2);
					}	
				}
			}
		}
	}
}