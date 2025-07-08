using System;
using System.Collections.Generic;
using System.Net;

namespace HexaNet.PeerConnection
{
	public class PeerDuelProtocolConnection<MessageEnum> where MessageEnum : Enum
	{
		public PeerTCPConnection<MessageEnum> tcp;
		public PeerUDPConnection<MessageEnum> udp;
		public readonly Dictionary<ulong, IPEndPoint> tcpPeerIds = new Dictionary<ulong, IPEndPoint>();
		public readonly Dictionary<ulong, IPEndPoint> udpPeerIds = new Dictionary<ulong, IPEndPoint>();

		public void AddTCPPeer(IPEndPoint peer, ulong id)
		{
			tcpPeerIds.Add(id, peer);
			tcp.AddPeer(peer, id);
		}

		public void AddUDPPeer(IPEndPoint peer, ulong id)
		{
			udpPeerIds.Add(id, peer);
			udp.AddPeer(peer, id);
		}

		public void RemovePeer(ulong peerId)
		{
			tcpPeerIds.Remove(peerId);
			udpPeerIds.Remove(peerId);
		}

		public IPEndPoint GetPeerEndPoint(ulong peerId, bool reliable = true)
		{
			if (reliable)
			{
				if (!tcpPeerIds.ContainsKey(peerId))
				{
					return null;
				}

				return tcpPeerIds[peerId];
			}
			else
			{
				if (!udpPeerIds.ContainsKey(peerId))
				{
					return null;
				}

				return udpPeerIds[peerId];
			}
		}

		public ulong GetPeerClientId(IPEndPoint peer)
		{
			if (!tcpPeerIds.ContainsValue(peer))
			{
				if (!udpPeerIds.ContainsValue(peer))
				{
					return 0;
				}

				return udpPeerIds.GetKeyOf(peer);
			}

			return tcpPeerIds.GetKeyOf(peer);
		}

		public void OnMessage(MessageEnum type, Action<NetMessage<MessageEnum>, IPEndPoint> action)
		{
			tcp.OnMessage(type, action);
			udp.OnMessage(type, action);
		}

		public void OnDisconnect(Action<IPEndPoint> action)
		{
			tcp.OnDisconnect(action);
		}

		public void Listen()
		{
			tcp.Listen();
			udp.Listen();
		}

		public void Connect()
		{
			tcp.Connect();
			udp.Connect();
		}

		public void Close()
		{
			tcp.Close();
			udp.Close();
		}

		public PeerDuelProtocolConnection(IPEndPoint remote) {
			tcp = new PeerTCPConnection<MessageEnum>(remote);
			udp = new PeerUDPConnection<MessageEnum>(remote);
		}
	}
}
