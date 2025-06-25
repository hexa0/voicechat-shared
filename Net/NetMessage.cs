using System;

namespace VoiceChatShared.Net
{
	public class NetMessage<MessageEnum> where MessageEnum : Enum
	{
		public static byte[] header = new byte[] { 0xFF, 0xAA, 0x00 };

		MessageEnum m_type;
		byte[] m_data;
		byte[] m_body;
		ulong m_clientId;
		byte m_flags = 0;

		/// <summary>  
		/// The type of message
		/// </summary>
		public MessageEnum Type
		{
			get => m_type;
		}

		/// <summary>  
		/// The body of the message  
		/// </summary>
		public byte[] Body
		{
			get => m_body;
		}

		/// <summary>  
		/// The raw contents of the message  
		/// </summary>
		public byte[] Raw
		{
			get
			{
				if (m_data != null)
				{
					return m_data;
				}
				else
				{
					Commit();
					return m_data;
				}
			}
		}

		/// <summary>  
		/// The client id who sent this message, null when not applicable.  
		/// </summary>  
		public ulong Client
		{
			get => m_clientId;
		}

		/// <summary>  
		/// Whether the server has overriten the client ID,
		/// this is done when messages are forwarded
		/// </summary>  
		public bool ClientOverrideFlag
		{
			get => (m_flags & 0x01) != 0;
			set
			{
				if (value)
					m_flags |= 0x01;
				else
					m_flags &= 0xFE;
			}
		}

		void Commit()
		{
			if (ClientOverrideFlag)
			{
				// header + flags (1 byte) + clientId (8 bytes) + type (2 bytes) + body
				m_data = new byte[header.Length + 1 + 8 + 2 + m_body.Length];
				Buffer.BlockCopy(header, 0, m_data, 0, header.Length);
				m_data[header.Length] = m_flags;
				Buffer.BlockCopy(BitConverter.GetBytes(m_clientId), 0, m_data, header.Length + 1, 8);
				Buffer.BlockCopy(BitConverter.GetBytes(Convert.ToUInt16(m_type)), 0, m_data, header.Length + 1 + 8, 2);
				Buffer.BlockCopy(m_body, 0, m_data, header.Length + 1 + 8 + 2, m_body.Length);
			}
			else
			{
				// header + flags (1 byte) + type (2 bytes) + body
				m_data = new byte[header.Length + 1 + 2 + m_body.Length];
				Buffer.BlockCopy(header, 0, m_data, 0, header.Length);
				m_data[header.Length] = m_flags;
				Buffer.BlockCopy(BitConverter.GetBytes(Convert.ToUInt16(m_type)), 0, m_data, header.Length + 1, 2);
				Buffer.BlockCopy(m_body, 0, m_data, header.Length + 1 + 2, m_body.Length);
			}
		}

		/// <summary>  
		/// Create a new NetMessage with a specific type and body.  
		/// </summary>  
		public NetMessage(MessageEnum type, byte[] body, byte flags = 0)
		{
			m_flags = flags;
			m_type = type;
			m_body = body;
		}

		/// <summary>  
		/// Create a new NetMessage with a specific type and body with a Client override 
		/// </summary>  
		public NetMessage(MessageEnum type, byte[] body, ulong client, byte flags = 0)
		{
			m_flags = flags;
			m_type = type;
			m_body = body;
			m_clientId = client;
			ClientOverrideFlag = true;
		}

		/// <summary>  
		/// Decode a NetMessage from raw data.  
		/// </summary>  
		/// <param name="clientId">
		/// the client ID to use if the message does not contain one.
		/// </param>
		public NetMessage(byte[] data, ulong clientId)
		{
			if (data.Length < header.Length || data[0] != header[0] || data[1] != header[1] || data[2] != header[2])
				throw new ArgumentException("Data does not contain a valid NetMessage header");

			if (data.Length < header.Length + 1)
				throw new ArgumentException("Data is too short to be a valid NetMessage");

			m_data = data;
			m_flags = data[header.Length];
			if (ClientOverrideFlag)
			{
				if (data.Length < header.Length + 1 + 8 + 2)
					throw new ArgumentException("Data is too short to contain a client ID as is now required by the m_flags (ClientOverrideFlag)");

				m_clientId = BitConverter.ToUInt64(data, header.Length + 1);
				m_type = (MessageEnum)Enum.ToObject(typeof(MessageEnum), BitConverter.ToUInt16(data, header.Length + 1 + 8));
				m_body = new byte[data.Length - header.Length - 1 - 8 - 2];
				Buffer.BlockCopy(data, header.Length + 1 + 8 + 2, m_body, 0, m_body.Length);
			}
			else
			{
				m_clientId = clientId;
				m_type = (MessageEnum)Enum.ToObject(typeof(MessageEnum), BitConverter.ToUInt16(data, header.Length + 1));
				m_body = new byte[data.Length - header.Length - 1 - 2];
				Buffer.BlockCopy(data, header.Length + 1 + 2, m_body, 0, m_body.Length);
			}
		}
	}
}
