using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using VoiceChatShared.Net.Disposables;

namespace VoiceChatShared.Net
{
	public class TCP<MessageEnum> : Net<MessageEnum> where MessageEnum : Enum
	{
		internal static int socketBufferSize = 1024 * 256;
		internal IPEndPoint endPoint;
		internal DisposableTCPClient clientSocket;
		internal DisposableTCPListener listenerSocket;
		NetworkStream stream;

		public IPEndPoint ClientEndPoint
		{
			get { return (IPEndPoint)clientSocket.Client.LocalEndPoint; }
		}

		private readonly byte[] receiveBuffer = new byte[socketBufferSize];

		public class Client<T> where T : Enum
		{
			public TcpClient client;
			public NetworkStream stream;
			public Socket socket;
			public TCP<T> tcp;
			public IPEndPoint endPoint;
			public Action<Client<T>> onDisconnect;

			private readonly byte[] receiveBuffer = new byte[socketBufferSize];

			bool connected = true;

			public bool CanContinueConnection()
			{
				return connected;
			}

			public bool ContinueConnection()
			{
				if (CanContinueConnection())
				{
					stream.BeginRead(receiveBuffer, 0, socketBufferSize, AcceptData, null);
					return true;
				}
				else
				{
					return false;
				}
			}

			public void Disconnect()
			{
				Console.WriteLine("TCP Client Connection Closed");
				connected = false;
				onDisconnect.Invoke(this);
			}

			public void AcceptData(IAsyncResult result)
			{
				try
				{
					int dataLength = stream.EndRead(result);
					byte[] data = new byte[dataLength];
					Array.Copy(receiveBuffer, data, dataLength);

					if (dataLength <= 0)
					{
						Disconnect();
					}
					else
					{
						tcp.Recieve(data, endPoint);
					}
				}
				catch
				{
					Disconnect();
				}

				ContinueConnection();
			}
		}

		public List<Client<MessageEnum>> connectedClients = new List<Client<MessageEnum>>();

		public TCP(IPEndPoint remote) : base()
		{
			endPoint = remote;
		}

		public override void Send(byte[] data, IPEndPoint client = null)
		{
#if NET9_0_OR_GREATER
			if (client != null)
			{
				foreach (var connectedClient in connectedClients)
				{
					if (connectedClient.endPoint == client)
					{
						connectedClient.stream.WriteAsync(data, 0, data.Length);
						break;
					}
				}
			}
			else
			{
				clientSocket.Client.SendAsync(data);
			}
#elif NET35
			if (client != null)
			{
				foreach (var connectedClient in connectedClients)
				{
					if (connectedClient.endPoint == client)
					{
						connectedClient.stream.BeginWrite(data, 0, data.Length, null, null);
						break;
					}
				}

				listenerSocket.Server.BeginSendTo(data, 0, data.Length, SocketFlags.None, client, null, null);
			}
			else
			{
				clientSocket.Client.BeginSend(data, 0, data.Length, SocketFlags.None, null, null);
			}
#endif
		}

		public override void ConnectTo(IPEndPoint remote)
		{
			Console.WriteLine($"TCP: Connected to {remote.Address.GetHashCode()}:{remote.Port}");

			clientSocket = new DisposableTCPClient();
			clientSocket.Client.ReceiveBufferSize = socketBufferSize;
			clientSocket.Client.SendBufferSize = socketBufferSize;

			clientSocket.BeginConnect(remote.Address, remote.Port, result =>
			{
				try { stream = clientSocket.GetStream(); } catch { }

				if (stream != null)
				{
					ContinueConnection();
				}
				else
				{
					Disconnected(endPoint);
				}
			}, null);
		}

		public override void ListenOn(IPEndPoint remote)
		{
			Console.WriteLine($"TCP: Listening On {remote.Address}:{remote.Port}");

			listenerSocket = new DisposableTCPListener(endPoint);
			listenerSocket.Server.ReceiveBufferSize = socketBufferSize;
			listenerSocket.Server.SendBufferSize = socketBufferSize;

			listenerSocket.Start();
			ContinueIncomingConnections();
		}

		bool CanContinueIncomingConnections()
		{
			if (listenerSocket.IsDisposed)
			{
				Console.WriteLine("TCP Server Closed");
				return false;
			}

			return true;
		}

		bool ContinueIncomingConnections()
		{
			if (CanContinueIncomingConnections())
			{
				listenerSocket.BeginAcceptTcpClient(AcceptClient, null);
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool Connected
		{
			get { return clientSocket != null && clientSocket.Connected; }
		}

		bool CanContinueConnection()
		{
			bool canContinue = !clientSocket.IsDisposed & clientSocket.Connected;

			if (!canContinue)
			{
				Console.WriteLine("TCP Server Connection Closed");
				Disconnected(endPoint);
			}

			return canContinue;
		}

		bool ContinueConnection()
		{
			if (CanContinueConnection())
			{
				stream.BeginRead(receiveBuffer, 0, socketBufferSize, AcceptData, null);
				return true;
			}
			else
			{
				return false;
			}
		}

		void Disconnect()
		{
			Console.WriteLine("TCP Closed");

			clientSocket?.Shutdown();
			listenerSocket?.Shutdown();

			Disconnected(endPoint);
		}


		void AcceptClient(IAsyncResult result)
		{
			if (!CanContinueIncomingConnections()) return;

			try
			{
				TcpClient tcpClient = listenerSocket.EndAcceptTcpClient(result);
				IPEndPoint endPoint = (IPEndPoint)tcpClient.Client.RemoteEndPoint;

				Console.WriteLine($"new TCP client {endPoint.Address.GetHashCode()}:{endPoint.Port}");

				tcpClient.Client.ReceiveBufferSize = socketBufferSize;
				tcpClient.Client.SendBufferSize = socketBufferSize;

				Client<MessageEnum> client = new Client<MessageEnum>()
				{
					tcp = this,
					onDisconnect = disconnectingClient => {
						connectedClients.Remove(disconnectingClient);
						Disconnected(disconnectingClient.endPoint);
					},
					client = tcpClient,
					stream = tcpClient.GetStream(),
					socket = tcpClient.Client,
					endPoint = endPoint
				};

				connectedClients.Add(client);

				client.ContinueConnection();
			}
			catch {	}

			ContinueIncomingConnections();
		}

		private void AcceptData(IAsyncResult result)
		{
			if (!CanContinueConnection()) return;

			try
			{
				int dataLength = stream.EndRead(result);
				byte[] data = new byte[dataLength];
				Array.Copy(receiveBuffer, data, dataLength);

				if (dataLength <= 0)
				{
					Disconnect();
				}
				else
				{
					Recieve(data, endPoint);
				}
			}
			catch
			{
				Disconnect();
			}

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
			clientSocket?.Shutdown();
			listenerSocket?.Shutdown();
		}

		public static int GetOpenPort()
		{
			TcpListener dummyListener = new TcpListener(IPAddress.Loopback, 0);
			dummyListener.Start();
			int availablePort = ((IPEndPoint)dummyListener.LocalEndpoint).Port;
			dummyListener.Stop();

			return availablePort;
		}

		public new bool Disposed
		{
			get
			{
				if (clientSocket != null)
				{
					return clientSocket.IsDisposed || !clientSocket.Connected;
				}
				else if (listenerSocket != null)
				{
					return listenerSocket.IsDisposed;
				}
				else
				{
					return true;
				}
			}
		}
	}
}
