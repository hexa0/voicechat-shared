using System;
using System.Net;
using System.Net.Sockets;

namespace VoiceChatShared.Net.Disposables
{
	class DisposableUDPClient : UdpClient
	{
		public bool IsDisposed { get; set; }
		protected override void Dispose(bool disposing)
		{
			IsDisposed = true;
			base.Dispose(disposing);
		}

		public void Shutdown()
		{
			if (IsDisposed) return;

			Console.WriteLine("Shutting down DisposableUDPClient.");

			Client.Shutdown(SocketShutdown.Both);
			Client.Close();
			Dispose(true);
		}

		public DisposableUDPClient() : base() { }
		public DisposableUDPClient(IPEndPoint endPoint) : base(endPoint) { }
	}
}