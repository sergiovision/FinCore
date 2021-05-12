using System;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using BusinessLogic;
using BusinessObjects;
using BusinessObjects.BusinessObjects;
using log4net;
using Microsoft.AspNetCore.Hosting;
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
            var message = "Caught TaskScheduler_UnobservedTaskException!!!!" + sender;
            Console.Write(message);
            Log.Error(message);
        }

        private static void HanldeGlobalExceptions()
        {
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            //Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            //Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e != null)
            {
                Log.Error("CurrentDomain_UnhandledException: " + e);
                if (e.ExceptionObject is Exception o)
                {
                    var errMsg = o.ToString();
                    Console.Write(errMsg);
                    Log.Error(errMsg);
                }
            }
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            try
            {
                Log.Error(e.ToString());
            }
            catch
            {
            }
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        public static void Main(string[] args)
        {
            HanldeGlobalExceptions();

            var rc = HostFactory.Run(x =>
            {
                x.SetDescription(Configuration.ServiceDescription);
                x.SetDisplayName(Configuration.ServiceDisplayName);
                x.SetServiceName(Configuration.ServiceName);
                x.RunAsLocalSystem();

                x.Service(factory => { return QuartzServer.Server; });
            });
            var exitCode = (int) Convert.ChangeType(rc, rc.GetTypeCode());
            var msg = string.Format(
                "------------------------------- FinCore Exited with code: (%d) --------------------------------",
                exitCode);
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
                        .UseUrls("http://*:" + xtradeConstants.WebBackend_PORT)
                        .UseStartup<Startup>();
                });
        }
    }
}