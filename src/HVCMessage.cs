namespace HexaVoiceChatShared
{
	public enum HVCMessage : byte // the message type specified after the header
	{
		// shared

		Handshake = 0,

		// general events sent between the game and the internal server

		PCMData = 10,
		PeakData,
		ConnectToRelay,
		DisconnectFromRelay,
		KeepTranscodeAlive,

		// audio events sent between the game and the internal server

		EmitterData = 20, // camera and emitter locations
		SetSpeakerDeviceId, // Device Number
		SetSpeakerBufferMillis, // BufferMilliseconds
		SetSpeakerBufferCount, // NumberOfBuffers

		// microphone events sent between the game and the internal server

		GetMicDevices = 30,
		SetRNNoiseEnabled,
		SetMicDeviceId, // DeviceNumber
		SetMicBufferMillis, // BufferMilliseconds
		SetMicBufferCount, // NumberOfBuffers
		SetMicChannels, // Channels
		SetBitrate,
		SetListening,

		// voice rooms, these are automatically created & destroyed when needed by the relay

		VoiceRoomJoin = 200, // join a lobby on the relay server
		VoiceRoomKeepAlive, // keep a lobby alive on the relay server
		VoiceRoomLeave, // leave a lobby on the relay server

		// audio data events sent between the internal server and the relay server

		Opus = 210,
		SpeakingStateUpdated,
	}
}
