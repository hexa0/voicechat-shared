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
	}
}