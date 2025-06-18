using System.Net;
using System.Net.Sockets;

class DisposableUDPClient : UdpClient
{
	// https://stackoverflow.com/questions/3457521/how-to-check-if-object-has-been-disposed-in-c-sharp
	public bool IsDisposed { get; set; }
	protected override void Dispose(bool disposing)
	{
		IsDisposed = true;
		base.Dispose(disposing);
	}

	public DisposableUDPClient() : base() { }
	public DisposableUDPClient(IPEndPoint endPoint) : base(endPoint) { }
}