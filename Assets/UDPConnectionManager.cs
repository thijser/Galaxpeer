using System;
using System.Net.Sockets;
using System.Net;

class UDPConnectionManager : ConnectionManager
{
	UdpClient socket;

	public UDPConnectionManager(int port = 12345)
	{
		socket = new UdpClient (port);
	}

	protected void Receive()
	{
		socket.BeginReceive (new AsyncCallback (onReceive), null);
	}

	protected void onReceive(IAsyncResult result)
	{
		IPEndPoint ip = new IPEndPoint (IPAddress.Any, 0);
		socket.EndReceive (result, ref ip);
		Receive ();
	}
}
