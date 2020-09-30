using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Autofac;
using BusinessObjects;
using log4net;
using NetCoreServer;
using Newtonsoft.Json;

namespace FinCore
{
    class TMessageSession : TcpSession
    {
        private ILog log;
        private ISignalHandler handler;
        private IMessagingServer mServer;
        public TMessageSession(TcpServer server, ILog l, ISignalHandler signalHandler) : base(server)
        {
            log = l;
            handler = signalHandler;
            mServer = (IMessagingServer)server;
        }

        protected override void OnConnected()
        {
            //log.Info($"TcpSocket sessionId {Id} connected!");
        }

        protected override void OnDisconnected()
        {
            //log.Info($"TcpSocket sessionId {Id} disconnected!");
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            string message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            try
            {
#if DEBUG
                log.Debug("Incoming: " + message);
#endif
                string[] strings = message.Split(new char[] { (char)0x1A });
                foreach(var str in strings)
                {
                    if (string.IsNullOrEmpty(str))
                        continue;
                    SignalInfo signal = JsonConvert.DeserializeObject<SignalInfo>(str);
                    if (signal != null)
                    {
                        handler.PostSignal(signal, mServer);
                    }
                }
            } catch(Exception e)
            {
                log.Error($"OnReceived: {message } e={e.ToString()}");
            }
        }

        protected override void OnError(SocketError error)
        {
            log.Info($"TcpSocket session caught an error with code {error}");
        }
    }

    public class TMessagingServer : TcpServer, IMessagingServer
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TMessagingServer));

        public TMessagingServer(IPAddress address, int port) : base(address, port) { }

        protected override TcpSession CreateSession() {
            if (Program.Container == null)
                return null;
            ISignalHandler handler = Program.Container.Resolve<ISignalHandler>();
            if (handler != null)
                return new TMessageSession(this, log, handler);
            return null;
        }

        protected override void OnError(SocketError error)
        {
            log.Error($"TcpSocket server caught an error with code {error}");
        }

        public bool MulticastText(string text)
        {
            return this.Multicast($"{text}{(char)0x1A}");
        }

    }

}
