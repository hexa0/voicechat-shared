using static HexaVoiceChatShared.HexaVoiceChat.Protocol;
using System.Text;
using System;

namespace HexaVoiceChatShared.MessageProtocol
{
	public class VoiceChatMessage
	{
		internal static byte[] BuildMessageHeader(VoiceChatMessageType type)
		{
			int length = magicHeader.Length + 1;
			byte[] header = new byte[length];

			Buffer.BlockCopy(magicHeader, 0, header, 0, magicHeader.Length);
			header[magicHeader.Length] = (byte)type;

			return header;
		}
		public static byte[] BuildMessage(VoiceChatMessageType type, byte[] body)
		{
			byte[] header = BuildMessageHeader(type);
			byte[] message = new byte[header.Length + body.Length + magicFooter.Length];

			Buffer.BlockCopy(header, 0, message, 0, header.Length);
			Buffer.BlockCopy(body, 0, message, header.Length, body.Length);
			Buffer.BlockCopy(magicFooter, 0, message, header.Length + body.Length, magicFooter.Length);

			return message;
		}
		internal static bool CheckForHeader(byte[] message)
		{
			var segment = new byte[magicHeader.Length];
			Buffer.BlockCopy(message, 0, segment, 0, magicHeader.Length);
			return magicHeader.SequenceEqual(segment);
		}
		internal static bool CheckForFooter(byte[] message)
		{
			return Encoding.ASCII.GetString(message, message.Length - magicFooter.Length, magicFooter.Length) == Encoding.ASCII.GetString(magicFooter);
		}
		public static DecodedVoiceChatMessage DecodeMessage(byte[] message, int length)
		{
			DecodedVoiceChatMessage decoded = new DecodedVoiceChatMessage();

			decoded.raw = new byte[length];
			Buffer.BlockCopy(message, 0, decoded.raw, 0, length);

			if (CheckForHeader(message))
			{
				decoded.type = (VoiceChatMessageType)message[magicHeader.Length];
				decoded.body = new byte[length - magicHeader.Length - magicFooter.Length - 1];
				Buffer.BlockCopy(message, magicHeader.Length + 1, decoded.body, 0, length - magicHeader.Length - magicFooter.Length - 1);
			}
			else
			{
				throw new Exception("got message with invalid magic");
			}

			return decoded;
		}
	}

	public class DecodedVoiceChatMessage
	{
		public VoiceChatMessageType type;
		public byte[] body;
		public byte[] raw;
	}
}
