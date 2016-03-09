using System;
using System.Net;

public class ConnectionMessage : Message
{
	public Guid UUID;
	public IPAddress IP;
	public ushort Port;

	public ConnectionMessage(Guid uuid, IPAddress ip, ushort port)
	{
		UUID = uuid;
		IP = ip;
		Port = port;
	}

	public override byte[] Serialize()
	{
		return StringToBytes ("");
	}
}
