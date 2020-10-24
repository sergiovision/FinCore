using BusinessObjects;
using log4net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NetCoreServer;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace FinCore
{
    class MessagingService : IMessagingService
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MessagingService));
        private static IMessagingServer _serv;
        private static IMessagingServer _localserv;
        private static IConfiguration configuration;

        public IMessagingServer Server
        {
            get { return _serv; }
        }

        public IMessagingServer LocalServer
        {
            get { return _localserv; }
        }

        public void Init()
        {
            if (Server == null)
            {
                try
                {
                    short Port = Int16.Parse(configuration["MessagingPort"]);
                    if (Port <= 0)
                        Port = 2021;
                    if (configuration["WSProtocol"] == "wss")
                    {
                        string pass = configuration["CertificatePassword"];
                        var context = new SslContext(SslProtocols.Tls12, new X509Certificate2("server.pfx", pass));
                        _serv = new SMessagingServer(context, IPAddress.Any, Port);
                    }
                    else
                    {
                        _serv = new MessagingServer(IPAddress.Any, Port);
                    }
                }
                catch (Exception e)
                {
                    log.Info(e.ToString());
                }
            }

            if (LocalServer == null)
            {
                try
                {
                    short Port = Int16.Parse(configuration["TcpPort"]);
                    if (Port <= 0)
                        Port = 2022;
                    Thread.Sleep(100);
                    _localserv = new TMessagingServer(IPAddress.Any, Port);
                }
                catch (Exception e)
                {
                    log.Info(e.ToString());
                }
            }
        }

        public MessagingService(IConfiguration config)
        {
            configuration = config;
            //Init();
        }

        public void SendMessage(WsMessage wsMessage)
        {
            wsMessage.From = "Server";
            var send = JsonConvert.SerializeObject(wsMessage);
            Server.MulticastText(send);
        }

        public void SendMessage<T>(WsMessageType type, T obj)
        {
            WsMessage wsMessage = new WsMessage();
            wsMessage.Type = type;
            wsMessage.From = "Server";
            wsMessage.Message = JsonConvert.SerializeObject(obj);
            var send = JsonConvert.SerializeObject(wsMessage);
            Server.MulticastText(send);
        }

        public void SendTcpMessage(WsMessage wsMessage)
        {
            wsMessage.From = "TcpServer";
            var send = JsonConvert.SerializeObject(wsMessage);
            LocalServer.MulticastText(send);
        }

        public void SendTcpMessage<T>(WsMessageType type, T obj)
        {
            WsMessage wsMessage = new WsMessage();
            wsMessage.Type = type;
            wsMessage.From = "TcpServer";
            wsMessage.Message = JsonConvert.SerializeObject(obj);
            var send = JsonConvert.SerializeObject(wsMessage);
            LocalServer.MulticastText(send);
        }

    }

    public class MessagingBackgroundService : BackgroundService
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MessagingBackgroundService));
        private IMessagingService service;

        public MessagingBackgroundService(IServiceProvider services)
        {
            Services = services;
        }

        public IServiceProvider Services { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            service = (IMessagingService)Services.GetService(typeof(IMessagingService));
            service.Init();
            service.Server.Start();
            service.LocalServer.Start();
            log.Info("MessagingBackgroundService ExecuteAsync done.");
            await Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            service.LocalServer.Stop();
            service.Server.Stop();
            log.Info("MessagingBackgroundService is stopped.");
            await Task.CompletedTask;
        }
    }
}
