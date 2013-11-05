using SocketService;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Client1
{
    class Program
    {
        private static ManualResetEvent _signal = new ManualResetEvent(false);
        private const int TotalCount = 100000;
        private static int _currentReceived = 0;
        private const string SampleMessage = "hello1";

        static void Main(String[] args)
        {
            var client = new ClientSocket().Connect("127.0.0.1", 11000).Start((reply) =>
            {
                _currentReceived++;
                //Console.WriteLine("received: {0}", reply);
                if (_currentReceived % 10000 == 0)
                {
                    Console.WriteLine(_currentReceived);
                }
                if (_currentReceived == TotalCount)
                {
                    _signal.Set();
                }
            });

            var watch = Stopwatch.StartNew();

            for (var index = 0; index < TotalCount; index++)
            {
                client.SendMessage(SampleMessage, (messageContent) =>
                {
                    //Console.WriteLine("sent:{0}", messageContent);
                });
            }

            _signal.WaitOne();
            Console.WriteLine(watch.ElapsedMilliseconds);
            Console.ReadLine();
        }
    }
}
