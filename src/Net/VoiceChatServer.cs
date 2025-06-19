using System.Net;
using System;

namespace HexaVoiceChatShared.Net
{
	public class VoiceChatServer : UDP
	{
		public VoiceChatServer(IPEndPoint remote) : base(remote, true)
		{
			Console.WriteLine($"VoiceChatServer: Started");
			Listen();
		}
	}
}
