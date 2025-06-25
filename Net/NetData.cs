using System;

namespace VoiceChatShared.Net
{
	public static class NetData
	{
		public static byte[] noData = new byte[0];

		public static byte[] As(byte value)
		{
			return new byte[] { value };
		}

		public static byte[] As(bool value)
		{
			return new byte[] { (byte)(value ? 0x01 : 0x00) };
		}

		public static byte[] As(uint value)
		{
			return BitConverter.GetBytes(value);
		}

		public static byte[] As(int value)
		{
			return BitConverter.GetBytes(value);
		}

		public static byte[] As(ushort value)
		{
			return BitConverter.GetBytes(value);
		}

		public static byte[] As(ulong value)
		{
			return BitConverter.GetBytes(value);
		}
	}
}
