using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using BusinessObjects;
using log4net;
using Microsoft.Extensions.Hosting;
using Topshelf;

namespace FinCore
{
    /// <summary>
    ///     The main server logic.
    /// </summary>
    public class QuartzServer : ServiceControl, IQuartzServer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(QuartzServer));
        private IHost webapi;
        private IMainService xtradeServer;
        private static QuartzServer server;

        private QuartzServer()
        {
        }

        public static QuartzServer Server {
            get {
                if (server == null)
                {
                    server = new QuartzServer();
                    return server;
                }
                return server;
            }
        }

        /// <summary>
        ///     Starts this instance, delegates to scheduler.
        /// </summary>
        public virtual void Start()
        {
            try
            {
                string[] args = { "" };
                webapi = Program.CreateHostBuilder(args).Build();
                webapi.StartAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(string.Format("Quaetz Server start failed: {0}", ex.Message), ex);
                throw;
            }
        }

        /// <summary>
        ///     Stops this instance, delegates to scheduler.
        /// </summary>
        public virtual void Stop()
        {
            try
            {
                if (xtradeServer != null)
                    xtradeServer.Dispose();
                if (webapi != null)
                    webapi.Dispose();
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Scheduler stop failed: {0}", ex.Message), ex);
                throw;
            }

            Log.Info("Scheduler shutdown complete");
        }

        /// <summary>
        ///     Pauses all activity in scheduler.
        /// </summary>
        public virtual void Pause()
        {
            xtradeServer.PauseScheduler();
        }

        /// <summary>
        ///     Resumes all activity in server.
        /// </summary>
        public void Resume()
        {
            xtradeServer.ResumeScheduler();
        }

        /// <summary>
        ///     TopShelf's method delegated to <see cref="Start()" />.
        /// </summary>
        public bool Start(HostControl hostControl)
        {
            Start();
            return true;
        }

        /// <summary>
        ///     TopShelf's method delegated to <see cref="Stop()" />.
        /// </summary>
        public bool Stop(HostControl hostControl)
        {
            Stop();
            return true;
        }

        /// <summary>
        ///     Initializes the instance of the <see cref="QuartzServer" /> class.
        /// </summary>
        public virtual void Initialize(string angularFolder, string envName)
        {
            try
            {
                XTradeConfig config = Program.Container.Resolve<XTradeConfig>();
                xtradeServer = Program.Container.Resolve<IMainService>();
                xtradeServer.Init(Program.Container);

                Log.Info(String.Format("Inited with AngularFolder: {0},  Env: {1}", angularFolder, envName));
            }
            catch (Exception e)
            {
                Log.Error("Server initialization failed:" + e.Message, e);
                throw;
            }
        }


        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void  Dispose()
        {
            //base.Dispose();
            xtradeServer.Dispose(); // no-op for now
        }

        /// <summary>
        ///     TopShelf's method delegated to <see cref="Pause()" />.
        /// </summary>
        public bool Pause(HostControl hostControl)
        {
            Pause();
            return true;
        }

        /// <summary>
        ///     TopShelf's method delegated to <see cref="Resume()" />.
        /// </summary>
        public bool Continue(HostControl hostControl)
        {
            Resume();
            return true;
        }
    }
}
