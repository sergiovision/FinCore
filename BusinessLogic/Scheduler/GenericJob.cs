using System;
using System.Threading.Tasks;
using Autofac;
using BusinessLogic.BusinessObjects;
using BusinessObjects;
using BusinessObjects.BusinessObjects;
using Quartz;

namespace BusinessLogic.Scheduler
{
    public abstract class GenericJob : IJob
    {
        protected readonly IWebLog log;

        private DateTimeOffset runTime;

        protected string strMessage;
        // protected bool bShouldBeStopped;

        protected GenericJob()
        {
            log = MainService.thisGlobal.Container.Resolve<IWebLog>();
            // bShouldBeStopped = false;
        }

        public abstract Task Execute(IJobExecutionContext context);

        public void SetMessage(string message)
        {
            strMessage = message;
        }

        public bool Begin(IJobExecutionContext context)
        {
            runTime = SystemTime.UtcNow();
            var key = context.JobDetail.Key;
            return false;
        }

        public async void Exit(IJobExecutionContext context)
        {
            var now = SystemTime.UtcNow();
            var duration = now - runTime;
            strMessage += ". For " + (long) duration.TotalMilliseconds + " ms. At " +
                          now.ToString(xtradeConstants.MTDATETIMEFORMAT) + " GMT";
            SchedulerService.LogJob(context, strMessage);
            if (log != null && !string.IsNullOrEmpty(strMessage))
                log.Log(strMessage);
            await Task.CompletedTask;
        }
    }
}