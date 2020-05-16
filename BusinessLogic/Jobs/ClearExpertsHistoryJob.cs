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
    // ClearExpertsHistoryJob 
    internal class ClearExpertsHistoryJob : GenericJob
    {
        protected static string strPath = "";
        protected IScheduler sched;
        protected IJobDetail thisJobDetail;

        public ClearExpertsHistoryJob()
        {
            log.Debug("ClearExpertsHistoryJob c-tor");
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
                if (MainService.thisGlobal.IsDebug())
                    log.Info("ClearExpertsHistoryJob: Cleaning History in all *.set files");

                string fileDir = MainService.thisGlobal.GetGlobalProp(xtradeConstants.SETTINGS_PROPERTY_MTCOMMONFILES);
                int ordersDeleted = 0;
                var setFiles = Directory.EnumerateFiles(fileDir, "*.set", SearchOption.AllDirectories);
                int filesCount = 0;
                if (setFiles != null)
                {
                    filesCount = setFiles.Count();
                    foreach (string currentFile in setFiles)
                        ordersDeleted += MainService.thisGlobal.DeleteHistoryOrders(currentFile);
                }

                SetMessage($"ClearExpertsHistoryJob : Files Processed: {filesCount} Deleted Orders: {ordersDeleted} ");
            }
            catch (Exception ex)
            {
                SetMessage($"ERROR: {ex.ToString()}");
            }

            Exit(context);
            await Task.CompletedTask;
        }
    }
}