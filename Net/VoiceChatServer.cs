using System;
using System.Net.Sockets;
using System.Net;
using HexaVoiceChatShared.MessageProtocol;

namespace HexaVoiceChatShared.Net
{
	public class VoiceChatServer : VoiceChatNetBase
	{
		public VoiceChatServer(IPEndPoint remote)
		{
			Console.WriteLine($"VoiceChatServer: Listening on port {remote.Port} from {remote.Address}");
			endPoint = remote;
			socket = new UdpClient(endPoint);
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
					Console.WriteLine($"VoiceChatMessageType of \"{message.type}\" wasn't handled by the VoiceChatServer");
				}
			};

			Connect(false);
		}
	}
}
