using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp5
{
    class Program
    {
        static void Main(string[] args)
        {
            Client client = new Client();
            client.Disconnected += (sh, a) =>
            {
                Console.WriteLine("切断されました。");
            };
            client.Connect(60128);
            client.Send("hoge");
            while (true)
            {
                client.Service();
                Thread.Sleep(100);
                {
                    GC.Collect();
                }
            }
        }
    }
}
