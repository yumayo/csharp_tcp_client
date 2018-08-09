using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp5
{
    public static class Logger
    {
        public static void Debug(string message)
        {
            Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] {message}");
        }
    }

    public class Client : IDisposable
    {
        public IPEndPoint ServerIPEndPoint { get; set; }
        private Socket Socket { get; set; }
        public const int BufferSize = 100000;
        public byte[] Buffer { get; } = new byte[BufferSize];

        public event EventHandler Connected;
        public event EventHandler Disconnected;

        private List<string> ReceiveDatas { get; } = new List<string>();

        public void Dispose()
        {
            this.Socket?.Disconnect(false);
            this.Socket?.Dispose();
        }

        public void Connect(int port)
        {
            try
            {
                this.ServerIPEndPoint = new IPEndPoint(IPAddress.Loopback, port);
                this.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                this.Socket.Connect(this.ServerIPEndPoint);

                Connected?.Invoke(this, EventArgs.Empty);

                this.Socket.BeginReceive(this.Buffer, 0, this.Buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
            }
            catch(Exception)
            {
                Disconnected?.Invoke(this, EventArgs.Empty);
                return;
            }
        }

        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            try
            {
                var byteSize = this.Socket.EndReceive(asyncResult);
                if (byteSize > 0)
                {
                    lock (ReceiveDatas)
                    {
                        ReceiveDatas.Add(Encoding.UTF8.GetString(this.Buffer, 0, byteSize));
                    }
                    this.Socket.BeginReceive(this.Buffer, 0, this.Buffer.Length, SocketFlags.None, ReceiveCallback, null);
                }
                else
                {
                    Disconnected?.Invoke(this, EventArgs.Empty);
                    Logger.Debug("データを受け取れませんでした。");
                }
            }
            catch (Exception)
            {
                Disconnected?.Invoke(this, EventArgs.Empty);
                Logger.Debug("データを受け取れませんでした。");
            }
        }

        public void Service()
        {
            lock(ReceiveDatas)
            {
                foreach (var data in ReceiveDatas)
                {
                    OnReceive(data);
                }
                ReceiveDatas.Clear();
            }
        }

        public void OnReceive(string message)
        {
            Logger.Debug(message);
        }

        public void Send(string message)
        {
            try
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                this.Socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, new AsyncCallback(SendCallback), null);
            }
            catch (Exception)
            {
                Logger.Debug("送れませんでした。Send");
            }
        }

        private void SendCallback(IAsyncResult asyncResult)
        {
            try
            {
                var byteSize = this.Socket.EndSend(asyncResult);
            }
            catch (Exception)
            {
                Logger.Debug("送れませんでした。SendCallback");
            }
        }
    }
}
