using System;
namespace SocketService
{
    public interface IClientSocket
    {
        IClientSocket Connect(string address, int port);
        IClientSocket Start(Action<string> replyMessageReceivedCallback);
        IClientSocket Shutdown();
        IClientSocket SendMessage(string messageContent, Action<string> messageSentCallback);
    }
}
