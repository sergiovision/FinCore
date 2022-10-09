using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using BusinessLogic.BusinessObjects;
using BusinessLogic.Repo;
using BusinessLogic.Repo.Domain;
using BusinessObjects;
using BusinessObjects.BusinessObjects;
using Quartz;

namespace BusinessLogic.Scheduler;

// JobSupervisor responsible for sheduling and manage all jobs in quartz
internal class JobSupervisor : IJob
{
    private readonly IWebLog log;
    private IScheduler sched;
    private IJobDetail thisJobDetail;

    public JobSupervisor()
    {
        log = MainService.thisGlobal.Container.Resolve<IWebLog>();
        log.Debug("JobSuperviser c-tor");
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            thisJobDetail = context.JobDetail;
            sched = context.Scheduler;
            log.Info("JobSuperviser: ------- Scheduling Jobs -------");

            // CronExpression decoder
            // https://www.freeformatter.com/cron-expression-generator-quartz.html

            var jobs2Check =
                MainService.thisGlobal.Container.Resolve<DataService>().GetDBActiveJobsList();
            UnscheduleObsoleteJobs(jobs2Check);
            addOrModifyJobs(jobs2Check);

#if DEBUG
            // Debug mode
#else
            // Release mode
            MainService.thisGlobal.RunJobNow("SYSTEM", "TerminalMonitoringJob");
#endif

            // ScheduleJobsStatic();
        }
        catch (Exception ex)
        {
            log.Info(string.Format("{0}***{0}Failed: {1}{0}***{0}", Environment.NewLine, ex.Message));
        }

        await Task.CompletedTask;
    }

    protected void UnscheduleObsoleteJobs(IEnumerable<DBJobs> jobs2Check)
    {
        try
        {
            var jobs = jobs2Check.Select(d => getJobKeyForJobDescription(d))
                .Where(d => !d.Equals(thisJobDetail.Key));
            if (jobs != null)
                MainService.thisGlobal.UnsheduleJobs(jobs);
        }
        catch (SchedulerException e)
        {
            log.Error("method: UnscheduleObsoleteJobs: unable to remove the jobs.", e);
        }
    }

    /**
     * From jobs2Check (a fresh list of JobDescription objects), addOrModifyJobs checks if jobs must be added or modified.
     * <p>
     *     @param jobs2Check list of JobDescription objects
     */
    protected void addOrModifyJobs(IEnumerable<DBJobs> jobs2Check)
    {
        foreach (var job in jobs2Check)
            try
            {
                if (!ScheduleJob(job.Classpath, job.Grp, job.Name, job.Cron))
                    if (MainService.thisGlobal.DeleteJob(getJobKeyForJobDescription(job)))
                        ScheduleJob(job.Classpath, job.Grp, job.Name, job.Cron);
            }
            catch (SchedulerException e)
            {
                log.Error(
                    "method: addOrModifyJobs: error when retrieving a jobDetail with this jobKey: " + job.Name, e);
            }
    }

    public JobKey getJobKeyForJobDescription(ScheduledJobInfo aJobDescription)
    {
        return new JobKey(aJobDescription.Name, aJobDescription.Group);
    }

    private JobKey getJobKeyForJobDescription(DBJobs aJobDescription)
    {
        return new JobKey(aJobDescription.Name, aJobDescription.Grp);
    }

    protected void ScheduleJobsStatic()
    {
        //ScheduleJob(typeof(OandaRatioJob).FullName, xtradeConstants.JOBGROUP_OPENPOSRATIO, "OandaRatio", "0 0 0/1 ? * MON-FRI *");
        //ScheduleJob(typeof(MyFXBookRatioJob).FullName, xtradeConstants.JOBGROUP_OPENPOSRATIO, "MyFXBookRatio", "0 0 0/1 ? * MON-FRI *");

        //ScheduleJob(typeof(ForexFactoryNewsJob).FullName, xtradeConstants.JOBGROUP_NEWS, "ForexFactoryNewsJob",
        //    "0 0 6 ? * MON-FRI *");

        log.Info("JobSuperviser: ------- Jobs Scheduled -------");
    }

    protected void SetTimeZoneForTrigger(ICronTrigger trigger)
    {
        var container = MainService.thisGlobal.Container;
        if (container != null)
        {
            var xtrade = container.Resolve<IMainService>();
            if (xtrade != null)
            {
                var tz = xtrade.GetBrokerTimeZone();
                trigger.TimeZone = tz;
            }
        }
    }

    public bool ScheduleJob(string typeClassName, string group, string name,
        string cron) // where TJobType : GenericJob, new()
    {
        var type = Type.GetType(typeClassName);
        if (type == null)
            return false;
        var job = JobBuilder.Create(type)
            .WithIdentity(name, group)
            .UsingJobData("Lock", "false")
            .StoreDurably()
            .Build();
        var exists = sched.CheckExists(job.Key);
        if (exists.Result)
            return false;
        var triggerName = name + "Trigger";
        SchedulerService.SetJobDataMap(job.Key, job.JobDataMap);
        var trigger = (ICronTrigger) TriggerBuilder.Create()
            .WithIdentity(triggerName, group)
            .WithCronSchedule(cron)
            // .WithPriority(1)
            .Build();
        SetTimeZoneForTrigger(trigger);

        var result = sched.ScheduleJob(job, trigger);
        var ft = result.Result;

        log.Info(job.Key + " scheduled at: " + ft.ToUniversalTime() + " and repeat on cron: " +
                 trigger.CronExpressionString);
        return true;
    }

    public void ScheduleJobWithParam<TJobType>(string group, string name, string cron, string param, string value)
        where TJobType : IJob, new()
    {
        var job = JobBuilder.Create<TJobType>()
            .WithIdentity(name, group)
            .UsingJobData("Lock", "false")
            .UsingJobData(param, value)
            .StoreDurably()
            .Build();
        var exists = sched.CheckExists(job.Key);
        if (exists.Result) return;
        var triggerName = name + "Trigger";
        SchedulerService.SetJobDataMap(job.Key, job.JobDataMap);
        var trigger = (ICronTrigger) TriggerBuilder.Create()
            .WithIdentity(triggerName, group)
            .WithCronSchedule(cron)
            //.WithPriority(1)
            .Build();

        SetTimeZoneForTrigger(trigger);

        var result = sched.ScheduleJob(job, trigger);
        var ft = result.Result;

        log.Info(job.Key + " scheduled at: " + ft.ToUniversalTime() + " and repeat on cron: " +
                 trigger.CronExpressionString);
    }

    public void ScheduleThriftJob<TJobType>(string group, string name, short port, int timeoutsec)
        where TJobType : GenericJob, new()
    {
        var job = JobBuilder.Create<TJobType>()
            .WithIdentity(name, group)
            .UsingJobData("port", port.ToString())
            .StoreDurably()
            .Build();
        var exists = sched.CheckExists(job.Key);
        if (exists.Result)
            sched.DeleteJob(job.Key);
        var triggerName = name + "Trigger";
        SchedulerService.SetJobDataMap(job.Key, job.JobDataMap);
        var trigger = TriggerBuilder.Create()
            .WithIdentity(triggerName, group)
            .StartAt(DateTime.Now.AddSeconds(timeoutsec))
            .Build();

        var result = sched.ScheduleJob(job, trigger);
        var ft = result.Result;

        log.Info(job.Key + " scheduled to start at " + ft);
    }
}
