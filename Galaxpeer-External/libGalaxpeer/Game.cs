
namespace Galaxpeer
{
	public static class Game
	{
		public static ConnectionManager ConnectionManager { get; private set; }

		public static void Init(ConnectionManager connectionManager)
		{
			ConnectionManager = connectionManager;
		}
	}
}
