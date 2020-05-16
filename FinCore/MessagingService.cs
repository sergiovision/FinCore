using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using BusinessObjects;
using BusinessObjects.BusinessObjects;
using log4net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NetCoreServer;
using Newtonsoft.Json;

namespace FinCore
{
    class MessagingService : IMessagingService
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MessagingService));
        private static IMessagingServer _serv; 

        public IMessagingServer Server
        {
            get { return _serv; }
        }

        public MessagingService(IConfiguration configuration)
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
                } catch (Exception e)
                {
                    log.Info(e.ToString());
                }
            }
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
    }

    public class MessagingBackgroundService : BackgroundService
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MessagingBackgroundService));
        private IMessagingService service;

        public MessagingBackgroundService(IServiceProvider services, IMessagingService serv)
        {
            Services = services;
            service = serv;
        }

        public IServiceProvider Services { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            service.Server.Start();
            log.Info("MessagingBackgroundService ExecuteAsync done.");
            await Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            service.Server.Stop();
            log.Info("MessagingBackgroundService is stopped.");
            await Task.CompletedTask;
        }
    }
}
