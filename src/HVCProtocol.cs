namespace HexaVoiceChatShared
{
	public static class HVCProtocol
	{
		public static byte[] magicHeader = new byte[] { 0xFF, 0xAA, 0x00 }; // the header magic to verify the request
		public static byte[] magicFooter = new byte[] { 0x00, 0xAA, 0xFF }; // the footer magic to end the request
	}
}
