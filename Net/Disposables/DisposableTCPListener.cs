using System;
using System.Net;
using System.Net.Sockets;

namespace VoiceChatShared.Net.Disposables
{
	class DisposableTCPListener : TcpListener
	{
		public bool IsDisposed { get; set; }
		protected void Stop(bool disposing)
		{
			IsDisposed = true;
			Stop();
		}

		public void Shutdown()
		{
			if (IsDisposed) return;

			Console.WriteLine("Shutting down DisposableTCPListener.");

			Server.Shutdown(SocketShutdown.Both);
			Server.Close();
			Stop(true);
		}

		public DisposableTCPListener(IPEndPoint endPoint) : base(endPoint) { }
	}
}