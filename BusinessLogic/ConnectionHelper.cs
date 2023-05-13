using System.IO;
using System.Net;
using Autofac;
using BusinessLogic.BusinessObjects;
using BusinessLogic.Repo.Domain;
using BusinessLogic.SQLDriver;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;

namespace BusinessLogic;

internal static class ConnectionHelper
{
    private static readonly object lockObject = new object();
    private static ISessionFactory _sessionFactory;
    private static XTradeConfig config;

    private static void InitConfig()
    {
        if (config == null)
            config = MainService.thisGlobal.Container.Resolve<XTradeConfig>();
    }

    public static ISession CreateNewSession()
    {
        if (_sessionFactory == null)
        {
            InitConfig();
            var connection = config.ConnectionString();

            if (config.ConnectionStringName().Contains("SQLite"))
            {
                
                // http://qaru.site/questions/754091/getting-fluent-nhibernate-to-work-with-sqlite
                var dbConfig = MsSqliteConfiguration.Standard.ConnectionString(connection).Dialect<CustomDialect>(); 
                //.Driver<SQLiteCustomDriver>();
                
                // SqliteConnection
                _sessionFactory = Fluently.Configure().Database(dbConfig)
                    .Mappings(m => m.FluentMappings.AddFromAssemblyOf<DBAdviser>())
                    .BuildSessionFactory();
                
            }
            else
            {
                var dbConfig = MySQLConfiguration.Standard.ConnectionString(connection);
                _sessionFactory = Fluently.Configure().Database(dbConfig)
                    .Mappings(m => m.FluentMappings.AddFromAssemblyOf<DBAdviser>())
                    .BuildSessionFactory();
            }
        }

        lock (lockObject) // Session is not thread safe thus - should be locked.
        {
            return _sessionFactory.OpenSession();
        }
    }
}
