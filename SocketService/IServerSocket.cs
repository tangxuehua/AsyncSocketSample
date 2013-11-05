using System;

namespace SocketService
{
    public interface IServerSocket
    {
        IServerSocket Listen(int backlog);
        IServerSocket Bind(string address, int port);
        IClientSocket Start(Func<string, string> messageReceivedCallback, Action<string> replyMessageSentCallback);
    }
}
