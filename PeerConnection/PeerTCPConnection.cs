using System.Net;
using System;
using System.Collections.Generic;

namespace HexaNet.PeerConnection
{
	public class PeerTCPConnection<MessageEnum> : TCP<MessageEnum> where MessageEnum : Enum
	{
		Dictionary<IPEndPoint, ulong> peerIds = new Dictionary<IPEndPoint, ulong>();

		public void AddPeer(IPEndPoint peer, ulong id)
		{
			peerIds.Add(peer, id);
		}

		public void RemovePeer(IPEndPoint peer)
		{
			if (peerIds.ContainsKey(peer))
			{
				peerIds.Remove(peer);
			}
		}

		public ulong GetPeerClientId(IPEndPoint peer)
		{
			if (!peerIds.ContainsKey(peer))
			{
				return 0;
			}

			return peerIds[peer];
		}

		public override ulong GetClientId(IPEndPoint from)
		{
			return GetPeerClientId(from);
		}

		public PeerTCPConnection(IPEndPoint remote) : base(remote) { }
	}
}