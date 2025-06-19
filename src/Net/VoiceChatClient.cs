using System.Net;
using System;

namespace HexaVoiceChatShared.Net
{
	public class VoiceChatClient : UDP
	{
		public VoiceChatClient(IPEndPoint remote) : base(remote, false)
		{
			Console.WriteLine($"VoiceChatClient: Started");
		}
	}
}
