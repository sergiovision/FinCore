using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using System.Linq;
using BusinessObjects;
using Quartz;
using System.Diagnostics;
using System.IO;
using BusinessLogic.BusinessObjects;
using BusinessLogic.Scheduler;

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

                SignalInfo signal_ActiveOrders =
                    MainService.thisGlobal.CreateSignal(SignalFlags.AllTerminals, 0, EnumSignals.SIGNAL_ACTIVE_ORDERS, 0);
                MainService.thisGlobal.PostSignalTo(signal_ActiveOrders);

                SignalInfo signal_UpdateRates =
                    MainService.thisGlobal.CreateSignal(SignalFlags.AllTerminals, 0, EnumSignals.SIGNAL_UPDATE_RATES, 0);
                signal_UpdateRates.Data = MainService.thisGlobal.GetRatesList();
                MainService.thisGlobal.PostSignalTo(signal_UpdateRates);

                SignalInfo signal_checkBalance =
                    MainService.thisGlobal.CreateSignal(SignalFlags.AllTerminals, 0, EnumSignals.SIGNAL_CHECK_BALANCE, 0);
                MainService.thisGlobal.PostSignalTo(signal_checkBalance);

                SignalInfo signal_History =
                    MainService.thisGlobal.CreateSignal(SignalFlags.AllTerminals, 0, EnumSignals.SIGNAL_DEALS_HISTORY, 0);
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