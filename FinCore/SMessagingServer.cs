using System.Net;
using System.Net.Sockets;
using System.Text;
using Autofac;
using BusinessObjects;
using BusinessObjects.BusinessObjects;
using log4net;
using NetCoreServer;
using Newtonsoft.Json;

namespace FinCore
{
    internal class SMessageSession : WssSession
    {
        private readonly ISignalHandler handler;
        private readonly ILog log;
        private readonly IMessagingServer mServer;

        public SMessageSession(WssServer server, ILog l, ISignalHandler signalHandler) : base(server)
        {
            log = l;
            handler = signalHandler;
            mServer = (IMessagingServer) server;
        }

        public override void OnWsConnected(HttpRequest request)
        {
            log.Info($"WebSocket sessionId {Id} connected!");
        }

        public override void OnWsDisconnected()
        {
            log.Info($"WebSocket sessionId {Id} disconnected!");
        }

        public override void OnWsReceived(byte[] buffer, long offset, long size)
        {
            var message = Encoding.UTF8.GetString(buffer, (int) offset, (int) size);
#if DEBUG
            log.Debug("Incoming: " + message);
#endif
            var wsMessage = JsonConvert.DeserializeObject<WsMessage>(message);
            if (wsMessage != null) handler.ProcessMessage(wsMessage, mServer);
        }

        protected override void OnError(SocketError error)
        {
            log.Info($"WebSocket session caught an error with code {error}");
        }
    }

    public class SMessagingServer : WssServer, IMessagingServer
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SMessagingServer));

        public SMessagingServer(SslContext ctx, IPAddress address, int port)
            : base(ctx, address, port)
        {
        }

        protected override SslSession CreateSession()
        {
            if (Program.Container == null)
                return null;
            var handler = Program.Container.Resolve<ISignalHandler>();
            return new SMessageSession(this, log, handler);
        }

        protected override void OnError(SocketError error)
        {
            log.Error($"WebSocket server caught an error with code {error}");
        }
    }
}