using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using HexaVoiceChatShared.MessageProtocol;
using static HexaVoiceChatShared.HexaVoiceChat.Protocol;
using System.Text;

// huge shoutout to https://github.com/tom-weiland/tcp-udp-networking/tree/tutorial-part3 for being like the only source i could reference for implementing this, i was loosing my mind trying to make this two-way

namespace HexaVoiceChatShared.Net
{
	public class VoiceChatNetBase
	{
		internal static int socketBufferSize = 1024 * 256;
		internal Dictionary<string, FragmentQueue> fragmentQueue = new Dictionary<string, FragmentQueue>();
		internal UdpClient socket;
		internal IPEndPoint endPoint;
		internal Action<DecodedVoiceChatMessage, IPEndPoint> onMessageAction;
		internal Dictionary<VoiceChatMessageType, Action<DecodedVoiceChatMessage, IPEndPoint>> onMessageActions = new Dictionary<VoiceChatMessageType, Action<DecodedVoiceChatMessage, IPEndPoint>>();
		
		public void SendMessage(byte[] data, IPEndPoint? client = null)
		{
			socket.SendAsync(data, data.Length, client);
			// socket.BeginSend(data, data.Length, client, null, null);
		}

		public void OnMessage(VoiceChatMessageType type, Action<DecodedVoiceChatMessage, IPEndPoint> action)
		{
			onMessageActions[type] = action;
		}

		public void Connect(bool connectToEndPoint)
		{
			if (connectToEndPoint)
			{
				Console.WriteLine($"VoiceChatClient: Connected to port {endPoint.Port} at {endPoint.Address}");
				socket.Connect(endPoint);
			}

			socket.BeginReceive(Recieve, null);

			socket.Client.ReceiveBufferSize = socketBufferSize;
			socket.Client.SendBufferSize = socketBufferSize;
		}

		public void SwitchToEndPoint(IPEndPoint newServer)
		{
			throw new Exception("calls to SwitchToEndPoint are unstable and as such have been disabled, please reconstruct the class itself.");

			//if (!newServer.Equals(endPoint))
			//{
			//	Close();
			//	socket = new UdpClient();
			//	socket.Client.ReceiveBufferSize = socketBufferSize;
			//	socket.Client.SendBufferSize = socketBufferSize;
			//	endPoint = newServer;
			//	Connect(true);
			//}
		}

		public void Close()
		{
			socket.Client.Close();
			socket.Close();
		}

		internal void Recieve(IAsyncResult result)
		{
			try
			{
				IPEndPoint from = new IPEndPoint(IPAddress.Any, 0);
				byte[] bytes = socket.EndReceive(result, ref from);

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
			}
			catch (Exception e)
			{
				// likely a throw when we change the relay server
				// Console.WriteLine(e);
			}

			try
			{
				socket.BeginReceive(Recieve, null);
			}
			catch (Exception e)
			{
				// also likely a throw from changing a relay server,
				// if this is throwing from other means that is BAD
				// Console.WriteLine(e);
			}
		}
	}
}
