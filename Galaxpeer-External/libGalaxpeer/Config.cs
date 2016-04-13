using System;
using System.Net;
using System.Collections.Generic;

namespace Galaxpeer
{
	/*
	 * --port 36963
	 * --connect 127.0.0.1:36963
	 * --printmessages
	 * --printconnections
	 * --printentities
	 * --measurephysics
	 */
	public class Config
	{
		public readonly int ListenPort = 0;
		public readonly IPAddress ConnectIP = IPAddress.Parse ("127.0.0.1");
		public readonly int ConnectPort = 36963;

		public readonly bool PrintMessages = false;
		public readonly bool PrintConnections = false;
		public readonly bool PrintEntities = false;

		public readonly int MeasureFrequency = 10000;
		public readonly bool MeasureAny = false;
		public readonly bool MeasurePhysics = false;
		public readonly bool MeasureEntityCount = false;
		public readonly bool MeasureConnectionsCount = false;
		public readonly bool MeasureMessageCount = false;
		public readonly bool MeasureMessageTime = false;
		public readonly bool MeasureFail = false;

		public readonly bool Fail = false;

		// I know, I know, real elegant. Works though.
		public Config (string[] arguments)
		{
			for (int i = 0; i < arguments.Length; i++) {
				if (arguments [i] == "--port") {
					try {
						ListenPort = UInt16.Parse (arguments [i + 1]);
						i++;
					} catch (Exception) {
						Console.WriteLine ("Failed to parse port");
					}
				} else if (arguments [i] == "--connect") {
					try {
						string[] parts = arguments [i + 1].Split (':');
						ConnectIP = IPAddress.Parse (parts [0]);
						ConnectPort = UInt16.Parse (parts [1]);
						i++;
					} catch (Exception) {
						Console.WriteLine ("Failed to parse address to connect to");
					}
				} else if (arguments [i] == "--printmessages") {
					PrintMessages = true;
				} else if (arguments [i] == "--printconnections") {
					PrintConnections = true;
				} else if (arguments [i] == "--printentities") {
					PrintEntities = true;
				} else if (arguments [i] == "--measurephysics") {
					MeasureAny = true;
					MeasurePhysics = true;
				} else if (arguments [i] == "--measureentities") {
					MeasureAny = true;
					MeasureEntityCount = true;
				} else if (arguments [i] == "--measureconnections") {
					MeasureAny = true;
					MeasureConnectionsCount = true;
				} else if (arguments [i] == "--measuremessagecount") {
					MeasureAny = true;
					MeasureMessageCount = true;
				} else if (arguments [i] == "--measuremessagetime") {
					MeasureAny = true;
					MeasureMessageTime = true;
				} else if (arguments [i] == "--measureall") {
					MeasureAny = true;
					MeasurePhysics = true;
					MeasureEntityCount = true;
					MeasureConnectionsCount = true;
					MeasureMessageCount = true;
					MeasureMessageTime = true;
				} else if (arguments [i] == "--measurefreq") {
					try {
						MeasureFrequency = int.Parse (arguments [i + 1]);
						i++;
					} catch (Exception) {
						Console.WriteLine ("Failed to parse measure frequency");
					}
				} else if (arguments [i] == "--fail") {
					Fail = true;
				} else if (arguments [i] == "--measurefail") {
					MeasureFail = true;
				}
			}
		}
	}
}
