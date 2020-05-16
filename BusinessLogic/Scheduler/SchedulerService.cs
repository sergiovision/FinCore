using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Threading.Tasks;
using System.Threading;
using Autofac;
using BusinessObjects;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using BusinessLogic.BusinessObjects;

namespace BusinessLogic.Scheduler
{
    internal class SchedulerService
    {
        public static IScheduler sched;
        protected static ISchedulerFactory sf;

        public static bool bInitialized;
        public static bool isClustered;
        protected static IWebLog log;

        public SchedulerService(IWebLog l)
        {
            bInitialized = false;
            isClustered = false;
            log = l;
        }

        public static JobDataMap GetJobDataMap(JobKey key)
        {
            IJobDetail jd = sched.GetJobDetail(key).Result;
            if (jd != null)
                return jd.JobDataMap;
            return null;
        }

        public static void SetJobDataMap(JobKey key, JobDataMap map)
        {
            var jobDetail = sched.GetJobDetail(key);
            IJobDetail jd = jobDetail.Result;
            if (jd == null)
                return;
            jd.JobDataMap.PutAll(map);
        }

        protected void FillPropertiesDefault(NameValueCollection properties)
        {
            properties["quartz.scheduler.instanceName"] = "DefaultQuartzScheduler";
            properties["quartz.scheduler.rmi.export"] = "false";
            properties["quartz.scheduler.rmi.proxy"] = "false";
            properties["quartz.scheduler.wrapJobExecutionInUserTransaction"] = "false";
            properties["quartz.threadPool.class"] = "Quartz.Simpl.SimpleThreadPool, Quartz";
            properties["quartz.threadPool.threadCount"] = "10";
            //properties["quartz.threadPool.threadPriority"] = "2";
            properties["quartz.jobStore.misfireThreshold"] = "60000";
            properties["quartz.jobStore.class"] = "Quartz.Simpl.RAMJobStore, Quartz";
        }


        public virtual bool Initialize()
        {
            log.Info("------- Initializing Scheduler -------------------");
            try
            {
                XTradeConfig config = MainService.thisGlobal.Container.Resolve<XTradeConfig>();
                var properties = config.Quartz();

                // First we must get a reference to a scheduler
                sf = new StdSchedulerFactory(properties);
                sched = sf.GetScheduler().Result;
                // All of the jobs have been added to the scheduler, but none of the jobs
                // will run until the scheduler has been started
                sched.Start();

                while (!sched.IsStarted)
                {
                    log.Info("Waiting for scheduler to start.");
                    Thread.Sleep(1000);
                }

                log.Info("IsStarted=" + sched.IsStarted);
                log.Info("InstanceId=" + sched.SchedulerInstanceId);
                log.Info("SchedulerName=" + sched.SchedulerName);
                SchedulerMetaData metadata = sched.GetMetaData().Result;
                log.Info("IS REMOTE (CLUSTERED )=" + metadata.SchedulerRemote);
                isClustered = metadata.SchedulerRemote;

                RunJobSupervisor();
            }
            catch (Exception ex)
            {
                log.Error("Error Initializing Scheduler: " + ex.Message);
                bInitialized = false;
                return bInitialized;
            }

            bInitialized = true;
            return bInitialized;
        }

        public static void RunJobSupervisor()
        {
            IJobDetail job = JobBuilder.Create<JobSupervisor>()
                //.OfType<JobSupervisor>()
                .WithIdentity("JobSupervisor", "DefaultGroup")
                .WithDescription("Main Job that starts and manages others")
                .UsingJobData("Lock", "false")
                .Build();

            if (!sched.CheckExists(job.Key).Result)
            {
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("JobSupervisorTrigger")
                    .ForJob(job)
                    .StartNow() // run once now
                    .Build();

                sched.ScheduleJob(job, trigger);
            }
        }

        public static void removeJobTriggers(JobKey jobKey)
        {
            var triggers = sched.GetTriggersOfJob(jobKey);
            var trigs = triggers.Result;
            foreach (ITrigger trigger in trigs) sched.UnscheduleJob(trigger.Key);
        }

        public void Shutdown()
        {
            if (isClustered)
            {
                log.Info("------- Cluster Disconnect, Not Shutting Down ---------------------");
                return;
            }

            // XTradeMQLServer.Stop();
            if (bInitialized)
            {
                log.Info("------- Shutting Down ---------------------");
                SchedulerMetaData metaData = sched.GetMetaData().Result;
                log.Info(string.Format("Executed {0} jobs.", metaData.NumberOfJobsExecuted));
                sched.Shutdown(true);
                log.Info("------- Shutdown Complete -----------------");
            }
        }

        // preffered method, use it
        public static object GetJobProp(IJobExecutionContext context, string prop)
        {
            IJobDetail detail = context.JobDetail;
            if (detail == null)
                return null;
            if (detail.JobDataMap == null)
                return null;
            return detail.JobDataMap.GetString(prop);
        }

        public static string GetJobProp(string group, string name, string prop)
        {
            var key = new JobKey(name, group);
            IJobDetail detail = sched.GetJobDetail(key).Result;
            string res = "";
            if (detail == null)
                return res;
            if (detail.JobDataMap == null)
                return res;
            res = detail.JobDataMap.GetString(prop);
            if (res == null)
                return "";
            return res;
        }

        public static void LogJob(IJobExecutionContext context, string strMessage)
        {
            IJobDetail detailc = context.JobDetail;
            if (detailc == null)
                return;
            if (detailc.JobDataMap == null)
                return;
            detailc.JobDataMap.Put("log", strMessage);
        }

        public static void SetRunning(IJobExecutionContext context, bool value)
        {
            IJobDetail detailc = context.JobDetail;
            if (detailc == null)
                return;
            if (detailc.JobDataMap == null)
                return;
            detailc.JobDataMap.Put("Running", value.ToString());
        }

        public static void SetJobProp(IJobExecutionContext context, string prop, object value)
        {
            IJobDetail detailc = context.JobDetail;
            if (detailc == null)
                return;
            if (detailc.JobDataMap == null)
                return;
            string valc = value.ToString();
            detailc.JobDataMap.Put(prop, valc);
        }

        public static void SetJobCronSchedule(string group, string name, string cron)
        {
            try
            {
                var key = new JobKey(name, group);
                // now store value in jobstore dictionary
                IJobDetail detail = sched.GetJobDetail(key).Result;
                if (detail == null)
                    return;

                ITrigger trigger = GetJobTrigger(group, name);
                if (trigger != null)
                {
                    TriggerKey triggerkey = trigger.Key;
                    string triggerName = trigger.Key.Name;
                    string triggerGroup = trigger.Key.Group;
                    //int Priority = trigger.Priority;

                    //removeJobTriggers(detail);
                    var newtrigger = (ICronTrigger) TriggerBuilder.Create()
                        .WithIdentity(triggerName, triggerGroup)
                        .WithCronSchedule(cron)
                        //.WithPriority(Priority)
                        .Build();

                    var container = MainService.thisGlobal.Container;
                    if (container != null)
                    {
                        var xtrade = container.Resolve<IMainService>();
                        if (xtrade != null)
                        {
                            TimeZoneInfo tz = xtrade.GetBrokerTimeZone();
                            newtrigger.TimeZone = tz;
                        }
                    }

                    DateTimeOffset? ft = sched.RescheduleJob(triggerkey, newtrigger).Result;
                    log.Info(key + " has been rescheduled");
                }
            }
            catch (Exception e)
            {
                log.Error("Error" + e.ToString());
            }
        }

        public static ITrigger GetJobTrigger(string group, string name)
        {
            var trigs = sched.GetTriggersOfJob(new JobKey(name, group)).Result;
            foreach (ITrigger trigger in trigs) return trigger;
            return null;
        }

        public static DateTime? GetJobNextTime(string group, string name)
        {
            ITrigger trig = GetJobTrigger(group, name);
            if (trig == null)
                return null;
            DateTimeOffset? next = trig.GetNextFireTimeUtc();
            if (next.HasValue)
                return next.Value.DateTime;
            return null;
        }

        public static DateTime? GetJobPrevTime(string group, string name)
        {
            ITrigger trig = GetJobTrigger(group, name);
            if (trig == null)
                return null;
            DateTimeOffset? next = trig.GetPreviousFireTimeUtc();
            if (next.HasValue)
                return next.Value.DateTime;
            return null;
        }

        public static void RunJobNow(JobKey key)
        {
            if (!bInitialized)
                return;
            if (!sched.CheckExists(key).Result)
                return;
            sched.TriggerJob(key);
        }

        public static void StopJobNow(JobKey key)
        {
            if (!bInitialized)
                return;
            if (!sched.CheckExists(key).Result)
                return;
            if (IsJobRunning(key)) sched.Interrupt(key);
        }


        public static List<ScheduledJobInfo> GetAllJobsList()
        {
            var list = new List<ScheduledJobInfo>();
            if (!bInitialized)
                return list;
            var jobGroups = sched.GetJobGroupNames().Result;
            var runninglist = GetRunningJobs();
            foreach (string group in jobGroups)
            {
                var keys = sched.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(group));
                foreach (JobKey key in keys.Result)
                {
                    IJobDetail detail = sched.GetJobDetail(key).Result;
                    if (detail == null)
                        continue;
                    var jobview = new ScheduledJobInfo();
                    jobview.Name = detail.Key.Name;
                    jobview.Group = detail.Key.Group;
                    if (runninglist.ContainsKey(jobview.Group + jobview.Name))
                        jobview.IsRunning = true;
                    string strMessage = detail.JobDataMap.GetString("log");
                    jobview.Log = strMessage;
                    var trigs = sched.GetTriggersOfJob(detail.Key).Result;
                    if (trigs != null)
                        foreach (ITrigger trigger in trigs)
                        {
                            DateTimeOffset? prev = trigger.GetPreviousFireTimeUtc();
                            if (prev.HasValue)
                                jobview.PrevTime = prev.Value.DateTime.ToBinary();
                            DateTimeOffset? next = trigger.GetNextFireTimeUtc();
                            if (next.HasValue)
                                jobview.NextTime = next.Value.DateTime.ToBinary();
                            var crontrig = trigger as ICronTrigger;
                            if (crontrig != null) jobview.Schedule = crontrig.CronExpressionString;
                        }

                    list.Add(jobview);
                }
            }

            return list;
        }

        public static Dictionary<string, ScheduledJobInfo> GetRunningJobs()
        {
            var list = new Dictionary<string, ScheduledJobInfo>();
            if (!bInitialized)
                return list;
            var ilist = sched.GetCurrentlyExecutingJobs();
            Task.WaitAll(ilist);
            foreach (IJobExecutionContext ic in ilist.Result)
            {
                var view = new ScheduledJobInfo();
                view.Group = ic.JobDetail.Key.Group;
                view.Name = ic.JobDetail.Key.Name;
                if (!list.ContainsKey(view.Group + view.Name))
                    list.Add(view.Group + view.Name, view);
            }

            return list;
        }

        public static bool IsJobRunning(JobKey jk)
        {
            if (!bInitialized)
                return false;
            var ilist = sched.GetCurrentlyExecutingJobs();
            Task.WaitAll(ilist);
            foreach (IJobExecutionContext ic in ilist.Result)
                if (ic.JobDetail.Key.Name == jk.Name && ic.JobDetail.Key.Group == jk.Group)
                    return true;

            return false;
        }
    }
}