using Autofac;
using Autofac.Extensions.DependencyInjection;
using BusinessLogic;
using BusinessObjects;
using log4net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
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

        static void HanldeGlobalExceptions()
        {
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            //Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            //Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e != null)
            {
                Log.Error("CurrentDomain_UnhandledException: " + e.ToString());
                if (e.ExceptionObject is Exception o)
                {
                    string errMsg = o.ToString();
                    Console.Write(errMsg);
                    Log.Error(errMsg);
                }
            }
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            try
            {
                Log.Error(e.ToString());
            }
            catch { }
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
