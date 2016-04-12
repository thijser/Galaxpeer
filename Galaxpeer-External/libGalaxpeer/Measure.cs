using System.Diagnostics;
using System;
using System.Threading;
using System.IO;

namespace Galaxpeer
{
	public class Measurements
	{
		Config config;

		struct MeasureData
		{
			public TimeSpan TotalTime;
			public TimeSpan PhysicsTime;
			public TimeSpan ReceiveTime;
			public int TotalConnections;
			public int ConnectionsInRoi;
			public int ConnectionsOutsideRoi;
			public int ClosestConnections;
			public int TotalEntities;
			public string ReceivedMessages;
			public string SentMessages;

			public override string ToString()
			{
				return string.Format ("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}", TotalTime.Ticks, PhysicsTime.Ticks, ReceiveTime.Ticks, TotalConnections, ConnectionsInRoi, ConnectionsOutsideRoi, ClosestConnections, TotalEntities, ReceivedMessages, SentMessages);
			}
		}

		private Stopwatch total_watch = new Stopwatch();
		private Timer measure_timer;
		private object measure_lock = new object ();
		private TextWriter file;
		public Measurements (Config config)
		{
			this.config = config;
			if (config.MeasureAny) {
				Console.CancelKeyPress += delegate {
					this.Cleanup ();
				};
				total_watch.Start ();

				file = new StreamWriter ("log.csv");
				file.WriteLine ("Total time,Physics time,Receive time,Total connections,Connections in ROI,Connections outside ROI,Number of closest clients,Total entities,Received messages,Sent messages");

				measure_timer = new Timer (CountAll, null, config.MeasureFrequency, config.MeasureFrequency);
			}
		}

		private void Cleanup()
		{
			total_watch.Stop ();
			Console.WriteLine ("Total time: {0}", total_watch.Elapsed);
			Console.WriteLine ("Receive time: {0}", receive_watch.Elapsed);
			Console.WriteLine ("Physics time: {0}", physics_watch.Elapsed);
			Console.WriteLine ("Received messages:");
			received_messages.ForEach ((char Id, int Count) => {
				Console.WriteLine("  {0}: {1}", Id, Count);
			});
			Console.WriteLine ("Sent messages:");
			sent_messages.ForEach ((char Id, int Count) => {
				Console.WriteLine("  {0}: {1}", Id, Count);
			});
			file.Close ();
		}

		private Stopwatch receive_watch = new Stopwatch();
		public void BeginReceive ()
		{
			if (config.MeasureMessageTime) {
				lock (measure_lock) {
					receive_watch.Start ();
				}
			}
		}

		public void EndReceive ()
		{
			if (config.MeasureMessageTime) {
				lock (measure_lock) {
					receive_watch.Stop ();
				}
			}
		}

		private ConcurrentDictionary<char, int> received_messages = new ConcurrentDictionary<char, int>();
		public void ReceivedMessage (char type)
		{
			if (config.MeasureMessageCount) {
				lock (measure_lock) {
					received_messages.Acquire (() => {
						int count = received_messages.Get (type);
						received_messages.Set (type, count + 1);
					});
				}
			}
		}

		private ConcurrentDictionary<char, int> sent_messages = new ConcurrentDictionary<char, int> ();
		public void SentMessage (char type) {
			if (config.MeasureMessageCount) {
				lock (measure_lock) {
					sent_messages.Acquire (() => {
						int count = sent_messages.Get (type);
						sent_messages.Set (type, count + 1);
					});
				}
			}
		}

		public void ClientConnected (Client client)
		{

		}

		public void ClientDisconnected (Client client)
		{

		}

		public void CreateEntity (MobileEntity entity)
		{

		}

		public void DestroyEntity (MobileEntity entity)
		{

		}

		private Stopwatch physics_watch = new Stopwatch();
		public void BeginPhysics ()
		{
			if (config.MeasurePhysics) {
				lock (measure_lock) {
					physics_watch.Start ();
				}
			}
		}

		public void EndPhysics ()
		{
			if (config.MeasurePhysics) {
				lock (measure_lock) {
					physics_watch.Stop ();
				}
			}
		}

		private string messageCount (ConcurrentDictionary<char, int> messages)
		{
			string str = "";
			messages.Acquire (() => {
				messages.ForEach ((char id, int count) => {
					str += id + ":" + count + " ";
				});
				messages.Clear ();
			});
			return str;
		}

		private void CountAll (object _)
		{
			lock (measure_lock) {
				MeasureData data = new MeasureData ();
				data.TotalTime = total_watch.Elapsed;
				if (config.MeasurePhysics) {
					bool was_running = physics_watch.IsRunning;

					if (was_running) {
						physics_watch.Stop ();
					}

					data.PhysicsTime = physics_watch.Elapsed;
					physics_watch.Reset ();

					if (was_running) {
						physics_watch.Start ();
					}
				}
				if (config.MeasureMessageTime) {
					bool was_running = receive_watch.IsRunning;

					if (was_running) {
						receive_watch.Stop ();
					}

					data.ReceiveTime = receive_watch.Elapsed;
					receive_watch.Reset ();

					if (was_running) {
						receive_watch.Start ();
					}
				}
				if (config.MeasureMessageCount) {
					data.ReceivedMessages = messageCount (received_messages);
					data.SentMessages = messageCount (sent_messages);
				}
				if (config.MeasureEntityCount) {
					data.TotalEntities = EntityManager.Entities.Count;
				}
				if (config.MeasureConnectionsCount) {
					data.TotalConnections = Client.Clients.Count;
					data.ConnectionsInRoi = Game.ConnectionManager.ClientsInRoi.Count;
					data.ConnectionsOutsideRoi = 0;
					data.ClosestConnections = 0;
					foreach (var client in Game.ConnectionManager.ClosestClients) {
						if (client != null) {
							data.ClosestConnections++;
							if (!Game.ConnectionManager.ClientsInRoi.ContainsKey (client.Uuid)) {
								data.ConnectionsOutsideRoi++;
							}
						}
					}
				}

				file.WriteLine (data.ToString ());
			}
		}
	}
}
