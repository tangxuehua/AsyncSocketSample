using System;
using System.Net.Sockets;
using System.Text;

namespace SocketService
{
    public class DefaultSocketService : ISocketService
    {
        public void SendMessage(Socket targetSocket, string messageContent, Action<string> messageSentCallback)
        {
            var message = BuildMessage(messageContent, Encoding.ASCII);
            if (message.Length > 0)
            {
                var context = new SendContext { TargetSocket = targetSocket, MessageContent = messageContent, MessageSentCallback = messageSentCallback };
                targetSocket.BeginSend(message, 0, message.Length, 0, new AsyncCallback(SendCallback), context);
            }
        }
        public void ReceiveMessage(Socket sourceSocket, Action<string> messageReceivedCallback)
        {
            ReceiveInternal(new ReceiveState
            {
                SourceSocket = sourceSocket,
                MessageReceivedCallback = messageReceivedCallback
            }, 4);
        }

        private void SendCallback(IAsyncResult asyncResult)
        {
            try
            {
                var context = (SendContext)asyncResult.AsyncState;
                context.TargetSocket.EndSend(asyncResult);
                context.MessageSentCallback(context.MessageContent);
            }
            catch (SocketException socketException)
            {
                Console.WriteLine("Socket send exception, ErrorCode:{0}", socketException.SocketErrorCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unknown socket send exception:{0}", ex);
            }
        }
        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            var receiveState = (ReceiveState)asyncResult.AsyncState;
            var sourceSocket = receiveState.SourceSocket;
            var stringBuilder = receiveState.StringBuilder;
            var bytesRead = 0;

            try
            {
                bytesRead = sourceSocket.EndReceive(asyncResult);
            }
            catch (SocketException socketException)
            {
                Console.WriteLine("Socket receive exception, ErrorCode:{0}", socketException.SocketErrorCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unknown socket receive exception:{0}", ex);
            }

            if (bytesRead > 0)
            {
                if (receiveState.MessageSize == null)
                {
                    receiveState.MessageSize = GetMessageLength(receiveState.Buffer);
                    var size = receiveState.MessageSize <= ReceiveState.BufferSize ? receiveState.MessageSize.Value : ReceiveState.BufferSize;
                    ReceiveInternal(receiveState, size);
                }
                else
                {
                    stringBuilder.Append(Encoding.ASCII.GetString(receiveState.Buffer, 0, bytesRead));
                    if (receiveState.StringBuilder.Length < receiveState.MessageSize.Value)
                    {
                        var remainSize = receiveState.MessageSize.Value - stringBuilder.Length;
                        var size = remainSize <= ReceiveState.BufferSize ? remainSize : ReceiveState.BufferSize;
                        ReceiveInternal(receiveState, size);
                    }
                    else
                    {
                        receiveState.MessageReceivedCallback(stringBuilder.ToString());
                        receiveState.MessageSize = null;
                        receiveState.StringBuilder.Clear();
                        ReceiveInternal(receiveState, 4);
                    }
                }
            }
        }

        private IAsyncResult ReceiveInternal(ReceiveState receiveState, int size)
        {
            return receiveState.SourceSocket.BeginReceive(receiveState.Buffer, 0, size, 0, ReceiveCallback, receiveState);
        }
        private static int GetMessageLength(byte[] buffer)
        {
            var data = new byte[4];
            for (var i = 0; i < 4; i++)
            {
                data[i] = buffer[i];
            }
            return BitConverter.ToInt32(data, 0);
        }
        private static byte[] BuildMessage(string content, Encoding encoding)
        {
            var data = encoding.GetBytes(content);
            var header = BitConverter.GetBytes(data.Length);
            var message = new byte[data.Length + header.Length];
            header.CopyTo(message, 0);
            data.CopyTo(message, header.Length);
            return message;
        }
    }
}
