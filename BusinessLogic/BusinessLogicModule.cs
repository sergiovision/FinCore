using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using BusinessLogic.BusinessObjects;
using BusinessLogic.Repo;
using BusinessLogic.Scheduler;
using BusinessObjects;

namespace BusinessLogic
{
    public class BusinessLogicModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<XTradeConfig>().AsSelf().SingleInstance();
            builder.RegisterType<SchedulerService>().AsSelf().SingleInstance();
            builder.RegisterType<MainService>().As<IMainService>().SingleInstance();
            builder.RegisterType<DataService>().AsSelf().SingleInstance();
            builder.RegisterType<ServerSignalsHandler>().As<ISignalHandler>().SingleInstance();
            builder.RegisterType<ProcessImpersonation>().AsSelf().SingleInstance();
        }
    }
}