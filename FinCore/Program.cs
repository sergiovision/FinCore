using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using BusinessLogic;
using BusinessObjects;
using log4net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Topshelf;
using Host = Microsoft.Extensions.Hosting.Host;

namespace FinCore
{
    public class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        public static ILifetimeScope Container { get; set; }

        public static void Main(string[] args)
        {
            var rc = HostFactory.Run(x =>
            {
                x.SetDescription(Configuration.ServiceDescription);
                x.SetDisplayName(Configuration.ServiceDisplayName);
                x.SetServiceName(Configuration.ServiceName);
                x.RunAsLocalSystem();
         
                x.Service(factory =>
                {
                    return QuartzServer.Server;
                });
            });
            var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
            Log.Info("**** Service Exited with code: " + exitCode + " ****");
            Environment.ExitCode = exitCode;            
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    builder.RegisterModule(new BusinessLogicModule());
                    builder.RegisterModule(new MainServerModule());
                    // builder.Populate(services);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                    .UseUrls("http://*:" + xtradeConstants.WebBackend_PORT.ToString())
                    .UseStartup<Startup>();
                });
        }

    }
}
