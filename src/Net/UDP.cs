using System.Net;
using HexaVoiceChatShared.MessageProtocol;
using System.Text;
using System.Collections.Generic;
using System;

// huge shoutout to https://github.com/tom-weiland/tcp-udp-networking/tree/tutorial-part3 for being like the only source i could reference for implementing this, i was loosing my mind trying to make this two-way

namespace HexaVoiceChatShared.Net
{
	public class UDP
	{
		internal static int socketBufferSize = 1024 * 256;
		internal Dictionary<string, FragmentQueue> fragmentQueue = new Dictionary<string, FragmentQueue>();
		internal DisposableUDPClient socket;
		internal IPEndPoint endPoint;
		internal Action<DecodedVoiceChatMessage, IPEndPoint> onMessageAction;
		internal Dictionary<HVCMessage, Action<DecodedVoiceChatMessage, IPEndPoint>> onMessageActions = new Dictionary<HVCMessage, Action<DecodedVoiceChatMessage, IPEndPoint>>();

		public UDP(IPEndPoint remote, bool bindToAddress)
		{
			endPoint = remote;

			if (bindToAddress) {
				socket = new DisposableUDPClient(endPoint);
			}
			else
			{
				socket = new DisposableUDPClient();
			}

			socket.Client.ReceiveBufferSize = socketBufferSize;
			socket.Client.SendBufferSize = socketBufferSize;
			onMessageAction = delegate (DecodedVoiceChatMessage message, IPEndPoint endPoint)
			{
				if (onMessageActions.ContainsKey(message.type))
				{
					onMessageActions[message.type].Invoke(message, endPoint);
				}
				else
				{
					Console.Error.WriteLine($"VoiceChatMessageType of \"{message.type}\" wasn't handled.");
				}
			};
		}

		/// <summary>
		/// Send a raw message, don't rely on this, use the helper methods to build messages easier
		/// </summary>
		/// <param name="data">The data to send.</param>
		/// <param name="client">(Optional) the IPEndPoint to send it to</param>
		public void Send(byte[] data, IPEndPoint client = null)
		{
#if NET9_0_OR_GREATER
			socket.SendAsync(data, data.Length, client);
#elif NET35
			socket.BeginSend(data, data.Length, client, null, null);
#endif
		}

		/// <summary>
		/// Send a blank message, use this for messages that don't need data.
		/// </summary>
		/// <param name="eventType">The event type to trigger.</param>
		/// <param name="client">(Optional) the IPEndPoint to send it to.</param>
		public void SendEventMessage(HVCMessage eventType, IPEndPoint client = null)
		{
			Send(VoiceChatMessage.BuildMessage(
				eventType,
				new byte[] { }
			), client);
		}

		/// <summary>
		/// Send a message with a type and data.
		/// </summary>
		/// <param name="eventType">The event type to trigger.</param>
		/// <param name="data">The data to send.</param>
		/// <param name="client">(Optional) the IPEndPoint to send it to.</param>
		public void SendMessage(HVCMessage eventType, byte[] data, IPEndPoint client = null)
		{
			Send(VoiceChatMessage.BuildMessage(
				eventType,
				data
			), client);
		}

		public static byte[] AsData(byte value)
		{
			return new byte[] { value };
		}

		public static byte[] AsData(bool value)
		{
			return new byte[] { (byte)(value ? 0x01 : 0x00) };
		}

		public static byte[] AsData(int value)
		{
			return BitConverter.GetBytes(value);
		}

		/// <summary>
		/// Send a message with a type, a client, and data.
		/// </summary>
		/// <param name="clientId">(Optional) the clientId the message is from.</param>
		/// <param name="eventType">The event type to trigger.</param>
		/// <param name="data">The data to send.</param>
		/// <param name="client">(Optional) the IPEndPoint to send it to.</param>
		public void SendClientWrappedMessage(ulong clientId, HVCMessage eventType, byte[] data, IPEndPoint client = null)
		{
			Send(ClientWrappedMessage.BuildMessage(
				clientId,
				eventType,
				data
			), client);
		}

		public void OnMessage(HVCMessage type, Action<DecodedVoiceChatMessage, IPEndPoint> action)
		{
			onMessageActions[type] = action;
		}

		public void OnClientMessage(HVCMessage type, Action<DecodedClientWrappedMessage, IPEndPoint> action)
		{
			onMessageActions[type] = (DecodedVoiceChatMessage message, IPEndPoint clientEndpoint) => action(ClientWrappedMessage.DecodeMessage(message.body), clientEndpoint);
		}

		public void Connect()
		{
			Console.WriteLine($"UDP: Connected to port {endPoint.Port} at {endPoint.Address}");
			StartConnect();
			StartListen();
		}

		public void Listen()
		{
			Console.WriteLine($"UDP: Listening on port {endPoint.Port} at {endPoint.Address}");
			StartListen();
		}

		internal void StartConnect()
		{
			socket.Connect(endPoint);
		}

		internal void StartListen()
		{
			socket.BeginReceive(Recieve, null);

			socket.Client.ReceiveBufferSize = socketBufferSize;
			socket.Client.SendBufferSize = socketBufferSize;
		}

		[Obsolete("calls to SwitchToEndPoint are unstable and as such have been disabled, please reconstruct the class itself.", true)]
		public void SwitchToEndPoint(IPEndPoint newServer)
		{
			if (!newServer.Equals(endPoint))
			{
				Close();

				socket = new DisposableUDPClient();
				socket.Client.ReceiveBufferSize = socketBufferSize;
				socket.Client.SendBufferSize = socketBufferSize;
				endPoint = newServer;

				Connect();
			}
		}

		public void Close()
		{
			socket.Shutdown();
		}

		internal void Recieve(IAsyncResult result)
		{
			void ContinueConnection()
			{
				if (!socket.IsDisposed)
				{
					socket.BeginReceive(Recieve, null);
				}
				else
				{
					Console.WriteLine("Connection Closed");
				}
			}

			if (socket.IsDisposed) { Console.WriteLine("Connection Closed"); return; }

			IPEndPoint from = new IPEndPoint(IPAddress.Any, 0);

			byte[] bytes;

			try { bytes = socket.EndReceive(result, ref from); }
			catch { ContinueConnection(); return; }

			FragmentQueue queue;

			if (!fragmentQueue.ContainsKey(from.Address.ToString()))
			{
				queue = new FragmentQueue();
				fragmentQueue.Add(from.Address.ToString(), queue);
			}
			else
			{
				queue = fragmentQueue[from.Address.ToString()];
			}

			try
			{
				Buffer.BlockCopy(bytes, 0, queue.data, queue.dataOffest, bytes.Length);
				queue.dataOffest += bytes.Length;

				if (VoiceChatMessage.CheckForFooter(bytes))
				{
					DecodedVoiceChatMessage message = VoiceChatMessage.DecodeMessage(queue.data, queue.dataOffest);

					if (HexaVoiceChat.logRecievedMessages)
					{
						Console.WriteLine($"from {from} : {Math.Round(queue.dataOffest / 128f, 3)} KiB, type: {message.type}");
					}

					queue.dataOffest = 0;

					try
					{
						onMessageAction.Invoke(message, from);
					}
					catch (Exception e)
					{
						Console.WriteLine($"an onMessage action failed:\n{e}");
					}
				}
			}
			catch (Exception exception)
			{
				queue.dataOffest = 0;
				Console.WriteLine($"Received broadcast from {from} : {Math.Round(bytes.Length / 128f, 3)} KiB, failed to decode {Encoding.ASCII.GetString(bytes)}, \n{exception}");
			}

			try
			{
				ContinueConnection();
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e);
				ContinueConnection();
			}
		}
	}
}
