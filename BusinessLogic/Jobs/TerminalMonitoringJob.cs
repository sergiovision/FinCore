using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using BusinessLogic.BusinessObjects;
using BusinessLogic.Repo;
using BusinessObjects;
using BusinessObjects.BusinessObjects;
using Quartz;

namespace BusinessLogic.Jobs;

    // TerminalMonitoringJob starts and monitors terminals 
    internal class TerminalMonitoringJob : IJob
    {
        protected static string strPath = "";
        protected IWebLog log;
        protected ProcessImpersonation procUtil;
        protected IScheduler sched;
        protected IJobDetail thisJobDetail;

        public TerminalMonitoringJob()
        {
            log = MainService.thisGlobal.Container.Resolve<IWebLog>();
            log.Debug("TerminalMonitoringJob c-tor");
            procUtil = MainService.thisGlobal.Container.Resolve<ProcessImpersonation>();
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                thisJobDetail = context.JobDetail;
            sched = context.Scheduler;
            if (Utils.IsDebug())
                log.Info("TerminalMonitoringJob: ------- Monitor Terminals -------");

            var dataService = MainService.thisGlobal.Container.Resolve<DataService>();

            var results = (IEnumerable<object>) dataService.GetObjects(EntitiesEnum.Terminal, false);

            results = results.Where(x => (x as Terminal).Retired == false &&
                                         (x as Terminal).Stopped == false);
            foreach (var resRow in results)
            {
                var oPath = (resRow as Terminal).FullPath;
                if (oPath != null)
                {
                    strPath = oPath;
                    var appName = Path.GetFileNameWithoutExtension(strPath);
                    var processlist = Process.GetProcessesByName(appName);
                    if (processlist == null || processlist.Length == 0)
                    {
                        procUtil.ExecuteAppAsLoggedOnUser(strPath, "");
                    }
                    else
                    {
                        var procL = processlist.Where(d =>
                            d.MainModule.FileName.Equals(strPath, StringComparison.InvariantCultureIgnoreCase));
                        if (Utils.HasAny(procL)) procUtil.ExecuteAppAsLoggedOnUser(strPath, "");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            log.Error($"TerminalMonitoringJob Failed: {ex}");
        }

        await Task.CompletedTask;
    }
}
