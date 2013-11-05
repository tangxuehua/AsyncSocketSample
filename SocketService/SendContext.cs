using System;
using System.Net.Sockets;

namespace SocketService
{
    public class SendContext
    {
        public Socket TargetSocket;
        public string MessageContent;
        public Action<string> MessageSentCallback;
    }
}
