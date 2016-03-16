/*using UnityEngine;

namespace Galaxpeer
{
	public class LocationMessage : Message
	{
		public Vector3 Position { get; private set; }
		public Quaternion Rotation { get; private set; }
		public Vector3 Velocity { get; private set; }

		public LocationMessage(Transform transform, Vector3 velocity)
		{
			Position = new Vector3 (transform.position.x, transform.position.y, transform.position.z);
			Rotation = new Quaternion (transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
			Velocity = new Vector3 (velocity.x, velocity.y, velocity.z);
		}

		public LocationMessage(string serialized)
		{

		}

		public override byte[] Serialize()
		{
			return StringToBytes("L" + Position.ToString () + Rotation.ToString () + Velocity.ToString ());
		}
	}
}
*/