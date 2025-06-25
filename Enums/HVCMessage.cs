namespace VoiceChatShared.Enums
{
	public enum HVCMessage // the message type specified after the header
	{
		// shared

		Handshake = 0,

		// general events sent between the game and the internal server

		PCMData,
		PeakData,
		ConnectToRelay,
		ConnectedToRelay,
		DisconnectFromRelay,

		// audio events sent between the game and the internal server

		EmitterData, // camera and emitter locations
		SetSpeakerDeviceId, // Device Number
		SetSpeakerBufferMillis, // BufferMilliseconds
		SetSpeakerBufferCount, // NumberOfBuffers

		// microphone events sent between the game and the internal server

		GetMicDevices,
		SetRNNoiseEnabled,
		SetMicDeviceId, // DeviceNumber
		SetMicBufferMillis, // BufferMilliseconds
		SetMicBufferCount, // NumberOfBuffers
		SetMicChannels, // Channels
		SetBitrate,
		SetListening,

		// voice rooms, these are automatically created & destroyed when needed by the relay

		VoiceRoomAllocatePeerId = 500, // when connecting, have the server allocate us a peerId
		VoiceRoomPeerIdAllocated, // the server has allocated us a peerId, this is sent back to the client
		VoiceRoomJoin, // join a voice room
		VoiceRoomPeersUpdated, // send the starting peers in a voice room to the client

		// audio data events sent between the internal server and the relay server

		Opus = 1000,
		SpeakingStateUpdated,
	}
}
