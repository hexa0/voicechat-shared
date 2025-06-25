namespace VoiceChatShared.Enums
{
	public enum RelayConnectionState : byte
	{
		Connecting,
		AllocatingId,
		Connected,
		Disconnected,
		Closed,
		Failed,
		TimedOut
	}
}
