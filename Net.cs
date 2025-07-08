
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace HexaNet
{
	public class Net<MessageEnum> : INet<MessageEnum> where MessageEnum : Enum
	{
		public class MessageAction
		{
			public MessageEnum type;
			public Action<NetMessage<MessageEnum>, IPEndPoint> action;
			
			public MessageAction(MessageEnum type, Action<NetMessage<MessageEnum>, IPEndPoint> action)
			{
				this.type = type;
				this.action = action;
			}
		}

		internal List<MessageAction> onMessageActions = new List<MessageAction>();
		internal List<Action<IPEndPoint>> onDisconnectActions = new List<Action<IPEndPoint>>();

		/// <summary>
		/// Send a blank message, use this for messages that don't need data.
		/// </summary>
		/// <param name="type">The event type to trigger.</param>
		/// <param name="client">(Optional) the IPEndPoint to send it to.</param>
		public void SendEventMessage(MessageEnum type, IPEndPoint client = null)
		{
			Send(new NetMessage<MessageEnum>(
				type,
				NetData.noData
			).Raw, client);
		}

		/// <summary>
		/// Send a message.
		/// </summary>
		/// <param name="message">The message to send.</param>
		/// <param name="client">(Optional) the IPEndPoint to send it to.</param>
		public void SendMessage(NetMessage<MessageEnum> message, IPEndPoint client = null)
		{
			Send(message.Raw, client);
		}

		/// <summary>
		/// Send a message with a type and data.
		/// </summary>
		/// <param name="type">The event type to trigger.</param>
		/// <param name="data">The data to send.</param>
		/// <param name="client">(Optional) the IPEndPoint to send it to.</param>
		public void SendMessage(MessageEnum type, byte[] data, IPEndPoint client = null)
		{
			SendMessage(new NetMessage<MessageEnum>(
				type,
				data
			), client);
		}

		/// <summary>
		/// Send a message with a type and data and a client override.
		/// </summary>
		/// <param name="type">The event type to trigger.</param>
		/// <param name="data">The data to send.</param>
		/// <param name="client">(Optional) the IPEndPoint to send it to.</param>
		public void SendMessage(MessageEnum type, byte[] data, ulong clientId, IPEndPoint client = null)
		{
			SendMessage(new NetMessage<MessageEnum>(
				type,
				data,
				clientId
			), client);
		}

		public virtual void Send(byte[] data, IPEndPoint destination = null)
		{
			throw new NotImplementedException();
		}

		public virtual ulong GetClientId(IPEndPoint from)
		{
			return 0;
		}

		public virtual void Recieve(byte[] data, IPEndPoint from)
		{
			NetMessage<MessageEnum> message = new NetMessage<MessageEnum>(data, GetClientId(from));

			bool match = false;

			lock (onMessageActions)
			{
				foreach (MessageAction action in onMessageActions)
				{
					if (EqualityComparer<MessageEnum>.Default.Equals(action.type, message.Type))
					{
						action.action.Invoke(message, from);
						match = true;
					}
				}
			}

			if (!match)
			{
				Console.Error.WriteLine($"Message of \"{message.Type}\" wasn't handled.");
			}
		}

		public virtual void ConnectTo(IPEndPoint endPoint)
		{
			throw new NotImplementedException();
		}

		public virtual void ListenOn(IPEndPoint endPoint)
		{
			throw new NotImplementedException();
		}

		public virtual void Close()
		{
			throw new NotImplementedException();
		}

		public void OnMessage(MessageEnum type, Action<NetMessage<MessageEnum>, IPEndPoint> action)
		{
			onMessageActions.Add(new MessageAction(type, action));
		}

		public void Once(MessageEnum type, Action<NetMessage<MessageEnum>, IPEndPoint> action)
		{
			bool first = true;
			MessageAction messageAction = null;
			Action<NetMessage<MessageEnum>, IPEndPoint> awaitAction = (message, peer) =>
			{
				if (!first) {
					new Thread(new ThreadStart(() =>
					{
						Thread.Sleep(100);

						lock(onMessageActions)
						{
							onMessageActions.Remove(messageAction);
						}
					})).Start();

					return;
				};

				first = false;
				action.Invoke(message, peer);
			};

			messageAction = new MessageAction(type, awaitAction);
			onMessageActions.Add(messageAction);
		}

		public virtual void Disconnected(IPEndPoint peer)
		{
			onDisconnectActions.ForEach(action => action.Invoke(peer));
		}

		public void OnDisconnect(Action<IPEndPoint> action)
		{
			onDisconnectActions.Add(action);
		}

		public bool Disposed
		{
			get { return false; }
		}
	}
}
