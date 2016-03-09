using System.Net.Sockets;

public abstract class Connection
{
	public Connection(ConnectionMessage message)
	{

	}

	public abstract void Send (Message message);
}
