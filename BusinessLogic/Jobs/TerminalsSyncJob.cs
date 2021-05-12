using System;
using System.Threading.Tasks;
using BusinessLogic.BusinessObjects;
using BusinessLogic.Scheduler;
using BusinessObjects;
using BusinessObjects.BusinessObjects;
using Quartz;

namespace BusinessLogic.Jobs
{
    internal class TerminalsSyncJob : GenericJob
    {
        protected IScheduler sched;
        protected IJobDetail thisJobDetail;

        public TerminalsSyncJob()
        {
            log.Debug("TerminalsSyncJob c-tor");
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

                var signal_ActiveOrders =
                    MainService.thisGlobal.CreateSignal(SignalFlags.AllTerminals, 0, EnumSignals.SIGNAL_ACTIVE_ORDERS,
                        0);
                MainService.thisGlobal.PostSignalTo(signal_ActiveOrders);

                var signal_UpdateRates =
                    MainService.thisGlobal.CreateSignal(SignalFlags.AllTerminals, 0, EnumSignals.SIGNAL_UPDATE_RATES,
                        0);
                signal_UpdateRates.SetData(MainService.thisGlobal.GetRatesList());
                MainService.thisGlobal.PostSignalTo(signal_UpdateRates);

                var signal_checkBalance =
                    MainService.thisGlobal.CreateSignal(SignalFlags.AllTerminals, 0, EnumSignals.SIGNAL_CHECK_BALANCE,
                        0);
                MainService.thisGlobal.PostSignalTo(signal_checkBalance);

                var signal_History =
                    MainService.thisGlobal.CreateSignal(SignalFlags.AllTerminals, 0, EnumSignals.SIGNAL_DEALS_HISTORY,
                        0);
                signal_History.Value = 2;
                MainService.thisGlobal.PostSignalTo(signal_History);


                SetMessage("TerminalsSyncJob Finished.");
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