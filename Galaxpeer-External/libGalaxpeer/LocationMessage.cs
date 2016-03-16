
namespace Galaxpeer
{
	public class LocationMessage : Message
	{
		public Vector3 Position { get; private set; }
		public Vector4 Rotation { get; private set; }
		public Vector3 Velocity { get; private set; }

		static LocationMessage()
		{
			MessageFactory.Register ('L', typeof(LocationMessage));
		}

		public LocationMessage(Vector3 position, Vector4 rotation, Vector3 velocity)
		{
			Position = position;
			Rotation = rotation;
			Velocity = velocity;
		}

		public LocationMessage(byte[] bytes)
		{

		}

		public override byte[] Serialize()
		{
			return StringToBytes("L" + Position.ToString () + Rotation.ToString () + Velocity.ToString ());
		}
	}
}
