using System;
using System.Net;
using System.Net.Sockets;
using VoiceChatShared.Net.Disposables;

namespace VoiceChatShared.Net
{
	public class UDP<MessageEnum> : Net<MessageEnum> where MessageEnum : Enum
	{
		internal static int socketBufferSize = 1024 * 256;
		internal IPEndPoint endPoint;
		internal DisposableUDPClient socket;

		public IPEndPoint ClientEndPoint
		{
			get { return (IPEndPoint)socket.Client.LocalEndPoint; }
		}

		public UDP(IPEndPoint remote) : base()
		{
			endPoint = remote;
		}

		public override void Send(byte[] data, IPEndPoint client = null)
		{
#if NET9_0_OR_GREATER
			socket.SendAsync(data, data.Length, client);
#elif NET35
			socket.BeginSend(data, data.Length, client, null, null);
#endif
		}

		bool CanContinueConnection()
		{
			if (socket.IsDisposed)
			{
				Console.WriteLine("UDP Connection Closed");
				return false;
			}

			return true;
		}

		bool ContinueConnection()
		{
			if (CanContinueConnection())
			{
				socket.BeginReceive(RecieveData, null);
				return true;
			}
			else
			{
				return false;
			}
		}

		void RecieveData(IAsyncResult result)
		{
			if (!CanContinueConnection()) return;

			IPEndPoint from = new IPEndPoint(IPAddress.Any, 0);

			byte[] bytes;

			try { bytes = socket.EndReceive(result, ref from); }
			catch { ContinueConnection(); return; }

			Recieve(bytes, from);
			ContinueConnection();
		}

		public override void ConnectTo(IPEndPoint remote) {
			Console.WriteLine($"UDP: Connected To {remote.Address.GetHashCode()}:{remote.Port}");
			socket = new DisposableUDPClient();
			socket.Connect(remote);
			ContinueConnection();
		}

		public override void ListenOn(IPEndPoint remote)
		{
			Console.WriteLine($"UDP: Listening On {remote.Address}:{remote.Port}");
			socket = new DisposableUDPClient(remote);
			socket.Client.ReceiveBufferSize = socketBufferSize;
			socket.Client.SendBufferSize = socketBufferSize;
			ContinueConnection();
		}

		public void Listen()
		{
			ListenOn(endPoint);
		}

		public void Connect()
		{
			ConnectTo(endPoint);
		}

		public override void Close()
		{
			socket.Shutdown();
		}

		public new void OnDisconnect(Action<IPEndPoint> action)
		{
			throw new NotImplementedException("UDP does not support OnDisconnect.");
		}

		public override ulong GetClientId(IPEndPoint from)
		{
			// this should be handled on a case by case basis in extensions of this class
			return 0;
		}

		public static int GetOpenPort()
		{
			using (UdpClient udpClient = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0)))
			{
				int availablePort = ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;

				return availablePort;
			}
		}

		public new bool Disposed
		{
			get { return socket.IsDisposed; }
		}
	}
}
