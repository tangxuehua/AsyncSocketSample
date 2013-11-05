using System;
using System.Net.Sockets;
using System.Text;

namespace SocketService
{
    public class ReceiveState
    {
        public Socket SourceSocket = null;
        public const int BufferSize = 1024;
        public byte[] Buffer = new byte[BufferSize];
        public StringBuilder StringBuilder = new StringBuilder();
        public int? MessageSize;
        public Action<string> MessageReceivedCallback;
    }
}
