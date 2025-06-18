using System;
using static HexaVoiceChatShared.HexaVoiceChat.Protocol;

namespace HexaVoiceChatShared.MessageProtocol
{
	public class ClientWrappedMessage
	{
		internal static byte[] BuildMessageHeader(ulong clientId, byte[] body)
		{
			return BitConverter.GetBytes(clientId);
		}
		public static byte[] BuildMessage(ulong clientId, VoiceChatMessageType type, byte[] body)
		{
			byte[] header = BuildMessageHeader(clientId, body);
			byte[] message = new byte[header.Length + body.Length];

			Buffer.BlockCopy(header, 0, message, 0, header.Length);
			Buffer.BlockCopy(body, 0, message, header.Length, body.Length);

			return VoiceChatMessage.BuildMessage(type, message);
		}
		public static DecodedClientWrappedMessage DecodeMessage(byte[] message)
		{
			DecodedClientWrappedMessage decoded = new DecodedClientWrappedMessage();

			decoded.clientId = BitConverter.ToUInt64(message, 0);
			decoded.body = new byte[message.Length - 8];
			Buffer.BlockCopy(message, 8, decoded.body, 0, message.Length - 8);

			return decoded;
		}
	}

	public class DecodedClientWrappedMessage
	{
		public UInt64 clientId;
		public byte[] body;
		public DecodedVoiceChatMessage voiceChatMessage;
	}
}
