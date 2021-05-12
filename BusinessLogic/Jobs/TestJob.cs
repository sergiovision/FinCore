using System;
using System.Threading.Tasks;
using BusinessLogic.Scheduler;
using Quartz;

namespace BusinessLogic.Jobs
{
    internal class TestJob : GenericJob
    {
        public static Random NotRandom = new Random();
        protected IScheduler sched;
        protected IJobDetail thisJobDetail;


        public TestJob()
        {
            log.Debug("TestJob c-tor");
        }

        protected Task CleanMemory()
        {
            var before = GC.GetTotalMemory(false);
            // Collect all generations of memory.
            GC.Collect();
            var after = GC.GetTotalMemory(true);
            log.Log(string.Format("Memory Cleanup From: {0:N0} => To {1:N0}",
                before, after));
            return Task.CompletedTask;
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            if (Begin(context))
            {
                SetMessage("Job Locked");
                Exit(context);
                return;
            }

            try
            {
                thisJobDetail = context.JobDetail;
                sched = context.Scheduler;

                //var log = MainService.thisGlobal.Container.Resolve<IWebLog>();
                //log.Log("Hello world");
                await CleanMemory();

                // MainService.thisGlobal.ClearPositions();
                //PositionInfo position = new PositionInfo();
                //position.Lots = 1;
                //position.Type = 0;
                //position.Symbol = "BRENT";

                //SignalInfo signal_Order = MainService.thisGlobal.CreateSignal(SignalFlags.Cluster, 1, EnumSignals.SIGNAL_MARKET_MANUAL_ORDER);
                //signal_Order.Value = 0;


                //long ticket = NotRandom.Next() * 1000;

                //PositionInfo pos = new PositionInfo { Symbol = "GOLD", Ticket = ticket, Lots = 0.01, Profit = new decimal(5) };

                //var termNotify = MainService.thisGlobal.Container.Resolve<ITerminalEvents>();
                //if (termNotify != null)
                //   termNotify.InsertPosition(pos);

                //SignalInfo signal_History =
                //    MainService.thisGlobal.CreateSignal(SignalFlags.AllTerminals, 0, EnumSignals.SIGNAL_DEALS_HISTORY);
                //signal_History.Value = 0;
                //MainService.thisGlobal.PostSignalTo(signal_History);

                //                while(!context.CancellationToken.IsCancellationRequested)
                //              {
                //                Thread.Sleep(500);
                //          }


                SetMessage("TestJob Finished.");
            }
            catch (Exception ex)
            {
                SetMessage($"ERROR: {ex}");
            }

            Exit(context);
            await Task.CompletedTask;
        }
    }
}