using log4net;

namespace FinCore
{
    /// <summary>
    ///     Configuration for the Quartz server.
    /// </summary>
    public class Configuration
    {
        private const string PrefixServerConfiguration = "FinCore.MainServer";
        private const string KeyServiceName = PrefixServerConfiguration + ".serviceName";
        private const string KeyServiceDisplayName = PrefixServerConfiguration + ".serviceDisplayName";
        private const string KeyServiceDescription = PrefixServerConfiguration + ".serviceDescription";
        private const string KeyServerImplementationType = PrefixServerConfiguration + ".type";

        private const string DefaultServiceName = "FinCoreMainServer";
        private const string DefaultServiceDisplayName = "FinCore Main Server";
        private const string DefaultServiceDescription = "FinCore Main Tasks Running Service";
        private static readonly ILog log = LogManager.GetLogger(typeof(Configuration));
        private static readonly string DefaultServerImplementationType = typeof(QuartzServer).AssemblyQualifiedName;

        /// <summary>
        ///     Initializes the <see cref="Configuration" /> class.
        /// </summary>
        static Configuration()
        {
        }

        /*
        public static NameValueCollection InitProperties()
        {
            NameValueCollection properties = new NameValueCollection();

            // Local Quartz Props
            //properties["quartz.scheduler.instanceName"] = "ServerScheduler"; // not needed
            properties["quartz.threadPool.type"] = "Quartz.Simpl.SimpleThreadPool, Quartz";
            properties["quartz.threadPool.threadCount"] = "10";
            properties["quartz.threadPool.threadPriority"] = "Normal";

            // DB props
            properties["quartz.jobStore.lockHandler.type"] = "Quartz.Impl.AdoJobStore.UpdateLockRowSemaphore, Quartz";
            properties["quartz.jobStore.driverDelegateType"] = "Quartz.Impl.AdoJobStore.SqlServerDelegate, Quartz";
            properties["quartz.jobStore.dataSource"] = "default";
            properties["quartz.dataSource.default.connectionString"] = DataSource;
            properties["quartz.dataSource.default.provider"] = "SqlServer-20";
            properties["quartz.jobStore.type"] = "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz";
            properties["quartz.jobStore.useProperties"] = "true";
            properties["quartz.jobStore.tablePrefix"] = "QRTZ_";


            // clustering export props
            properties["quartz.scheduler.exporter.type"] = "Quartz.Simpl.RemotingSchedulerExporter, Quartz";
            properties["quartz.scheduler.exporter.port"] = "555";
            properties["quartz.scheduler.exporter.bindName"] = "QuartzScheduler";
            properties["quartz.scheduler.exporter.channelType"] = "tcp";
            properties["quartz.scheduler.exporter.channelName"] = "httpQuartz";
            return properties;
        */

        /// <summary>
        ///     Gets the name of the service.
        /// </summary>
        /// <value>The name of the service.</value>
        public static string ServiceName => GetConfigurationOrDefault(KeyServiceName, DefaultServiceName);

        /// <summary>
        ///     Gets the display name of the service.
        /// </summary>
        /// <value>The display name of the service.</value>
        public static string ServiceDisplayName =>
            GetConfigurationOrDefault(KeyServiceDisplayName, DefaultServiceDisplayName);

        /// <summary>
        ///     Gets the service description.
        /// </summary>
        /// <value>The service description.</value>
        public static string ServiceDescription =>
            GetConfigurationOrDefault(KeyServiceDescription, DefaultServiceDescription);

        /// <summary>
        ///     Gets the type name of the server implementation.
        /// </summary>
        /// <value>The type of the server implementation.</value>
        public static string ServerImplementationType =>
            GetConfigurationOrDefault(KeyServerImplementationType, DefaultServerImplementationType);

        /// <summary>
        ///     Returns configuration value with given key. If configuration
        ///     for the does not exists, return the default value.
        /// </summary>
        /// <param name="configurationKey">Key to read configuration with.</param>
        /// <param name="defaultValue">Default value to return if configuration is not found</param>
        /// <returns>The configuration value.</returns>
        private static string GetConfigurationOrDefault(string configurationKey, string defaultValue)
        {
            string retValue = null;
            //if (configuration != null)
            //{
            //    retValue = configuration[configurationKey];
            //}

            if (retValue == null || retValue.Trim().Length == 0) retValue = defaultValue;
            return retValue;
        }
    }
}