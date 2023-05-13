using System;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using BusinessObjects;
using BusinessObjects.BusinessObjects;
using log4net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NetCoreServer;
using Newtonsoft.Json;

namespace FinCore;

internal class MessagingService : IMessagingService
{
    private static readonly ILog log = LogManager.GetLogger(typeof(MessagingService));
    private static IMessagingServer _serv;
    private static IMessagingServer _localserv;
    private static IConfiguration configuration;

    public MessagingService(IConfiguration config)
    {
        configuration = config;
        //Init();
    }

    public IMessagingServer Server => _serv;

    public IMessagingServer LocalServer => _localserv;

    public void Init()
    {
        if (Server == null)
            try
            {
                var Port = short.Parse(configuration["MessagingPort"]);
                if (Port <= 0)
                    Port = 2021;
                if (configuration["WSProtocol"] == "wss")
                {
                    var pass = configuration["CertificatePassword"];
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

        if (LocalServer == null)
            try
            {
                var Port = short.Parse(configuration["TcpPort"]);
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

    public void SendMessage(WsMessage wsMessage)
    {
        wsMessage.From = "Server";
        var send = JsonConvert.SerializeObject(wsMessage);
        Server.MulticastText(send);
    }

    public void SendMessage<T>(WsMessageType type, T obj)
    {
        var wsMessage = new WsMessage();
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
        var wsMessage = new WsMessage();
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
    private Thread thread;

    public MessagingBackgroundService(IServiceProvider services)
    {
        Services = services;
    }

    public IServiceProvider Services { get; }

    private void RunThread()
    {
        Thread.Sleep(xtradeConstants.SERVICE_DELAY_MS);
        service = (IMessagingService) Services.GetService(typeof(IMessagingService));
        if (service != null)
        {
            service.Init();
            service.Server.Start();
            service.LocalServer.Start();
            log.Info("MessagingService started after delay of " + xtradeConstants.SERVICE_DELAY_MS + " ms...");
        }
        else
        {
            log.Info("MessagingService Failed to start!");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        thread = new Thread(RunThread);
        thread.Start();
        await Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        service.LocalServer.Stop();
        service.Server.Stop();
        log.Info("MessagingService is stopped.");
        await Task.CompletedTask;
    }
}
