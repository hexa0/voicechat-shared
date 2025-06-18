using System.Text;

namespace HexaVoiceChatShared
{
	public static class HexaVoiceChat
	{
		public static bool logRecievedMessages = false;
		public static class Ports
		{
			private const int basePort = 39200;
			public const int transcode = basePort + 0;
			public const int relay = basePort + 1;
		}

		public static class Protocol
		{
			public static byte[] magicHeader = Encoding.ASCII.GetBytes("hmMessageBegin"); // the header magic to verify the request
			public static byte[] magicFooter = Encoding.ASCII.GetBytes("hmEndMessage"); // the footer magic to end the request
			public enum VoiceChatMessageType : byte // the message type specified after the header
			{
				// voice rooms, these are automatically created & destroyed when needed by the relay

				VoiceRoomJoin = 0, // join a lobby on the relay server
				VoiceRoomKeepAlive, // keep a lobby alive on the relay server
				VoiceRoomLeave, // leave a lobby on the relay server

				// audio data events sent between the game and the internal server

				PCMData = 10,
				SwitchRelay,
				KeepTranscodeAlive,

				SetRNNoiseEnabled,
				SetMicDeviceId, // DeviceNumber
				SetMicBufferMillis, // BufferMilliseconds
				SetMicBufferCount, // NumberOfBuffers
				SetBitrate,

				// audio data events sent between the internal server and the relay server

				Opus = 20,
				SpeakingStateUpdated,

				// shared

				Handshake = 30,
			}
		}
	}
}