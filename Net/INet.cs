using System;
using System.Net;

namespace VoiceChatShared.Net
{
	internal interface INet<MessageEnum> where MessageEnum : Enum
	{
		/// <summary>
		/// Send a raw message, don't rely on this,
		/// instead use the helper methods to send messages
		/// </summary>
		/// <param name="data">The data to send.</param>
		/// <param name="destination">(Optional) the IPEndPoint to send it to</param>
		void Send(byte[] data, IPEndPoint destination = null);

		/// <summary>
		/// Get the client id of a specific IPEndPoint,
		/// this is called by the network implementation, you should not call this yourself
		/// </summary>
		ulong GetClientId(IPEndPoint from);

		/// <summary>
		/// Called with the data that was recieved,
		/// this is called by the network implementation, you should not call this yourself
		/// </summary>
		/// <param name="data">The recieved data.</param>
		/// <param name="from">the IPEndPoint that sent this data</param>
		void Recieve(byte[] data, IPEndPoint from);

		/// <summary>
		/// Called when a peer disconnects under supported protocols,
		/// this is called by the network implementation, you should not call this yourself
		/// </summary>
		/// <param name="peer">the IPEndPoint that disconnected</param>
		void Disconnected(IPEndPoint peer);

		/// <summary>
		/// Connect to a remote endpoint,
		/// this is called by the network implementation, you should not call this yourself
		/// /// </summary>
		void ConnectTo(IPEndPoint endPoint);

		/// <summary>
		/// Listen on a endpoint,
		/// this is called by the network implementation, you should not call this yourself
		/// /// </summary>
		void ListenOn(IPEndPoint endPoint);

		/// <summary>
		/// Close the connection.
		/// /// </summary>
		void Close();
	}
}
