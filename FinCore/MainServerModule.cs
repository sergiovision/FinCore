using System.Diagnostics;
using System.IO;
using System.Reflection;
using Autofac;
using AutoMapper;
using BusinessLogic;
using BusinessObjects;
using log4net;
using log4net.Config;
using Module = Autofac.Module;

namespace FinCore;

public class MainServerModule : Module
{
    protected void InitLogging()
    {
        var logRepo = LogManager.GetRepository(Assembly.GetAssembly(typeof(MainServerModule)));
        var processModule = Process.GetCurrentProcess().MainModule;
        if (processModule != null)
        {
            var pathToExe = processModule.FileName;
            var currentDir = Path.GetDirectoryName(pathToExe);
            var logFile = Path.Combine(currentDir, "log4net.config");
            XmlConfigurator.Configure(logRepo, new FileInfo(logFile));
        }
    }

    protected override void Load(ContainerBuilder builder)
    {
        InitLogging();
        builder.RegisterInstance(QuartzServer.Server);
        builder.RegisterType<PositionsManager>().As<ITerminalEvents>().SingleInstance();
        builder.RegisterType<WebLogManager>().As<IWebLog>().SingleInstance();
        builder.RegisterType<CryptoPosManager>().Named<ITerminalEvents>("crypto").SingleInstance();
        RegisterMaps(builder);
    }

    private static void RegisterMaps(ContainerBuilder builder)
    {
        builder.Register(ctx => new MapperConfiguration(cfg =>
        {
            var profile = new MappingProfile();
            cfg.AddProfile(profile);
        }));
        builder.Register(ctx => ctx.Resolve<MapperConfiguration>().CreateMapper()).As<IMapper>()
            .InstancePerLifetimeScope();
    }
}
