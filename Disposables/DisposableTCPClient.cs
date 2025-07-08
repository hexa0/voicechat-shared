using System;
using System.Net;
using System.Net.Sockets;

namespace HexaNet.Disposables
{
	class DisposableTCPClient : TcpClient
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

			Console.WriteLine("Shutting down DisposableTCPClient.");

			try
			{
				// Set LingerState to close socket immediately to avoid hang
				if (Client != null)
				{
					Client.LingerState = new LingerOption(true, 0);
					Client.Shutdown(SocketShutdown.Both);
				}
			}
			catch (SocketException)
			{
				// Data send/receive was disallowed
			}
			catch (ObjectDisposedException)
			{
				// Socket already disposed, ignore
			}
			finally
			{
				Client.Close();
				Dispose(true);
			}
		}

		public DisposableTCPClient() : base() { }
		public DisposableTCPClient(IPEndPoint endPoint) : base(endPoint) { }
	}
}