﻿
using System;

namespace Galaxpeer
{
	public static class Game
	{
		public static ConnectionManager ConnectionManager { get; private set; }
		public static Config Config { get; private set; }

		public static void Init(Config config, ConnectionManager connectionManager)
		{
			Config = config;
			ConnectionManager = connectionManager;
			PsycicManager.Instance.Init ();

			connectionManager.Connect (new ConnectionMessage (LocalPlayer.Instance.Uuid, config.ConnectIP, config.ConnectPort, LocalPlayer.Instance.Location));
		}
	}
}
