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

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            String message = "Caught TaskScheduler_UnobservedTaskException!!!!" + sender.ToString();
            Console.Write(message);
            Log.Error(message);
        }

        public static void Main(string[] args)
        {
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

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
            string msg = string.Format("------------------------------- FinCore Exited with code: (%d) --------------------------------", exitCode);
            Log.Info(msg);
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
