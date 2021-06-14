namespace BusinessObjects.BusinessObjects
{
    public static class xtradeConstants
    {
        public const string FINCORE_VERSION = "0.9.9";
        public const double GAP_VALUE = -125;
        public const string MTDATETIMEFORMAT = "yyyy.MM.dd HH:mm";
        public const string MYSQLDATETIMEFORMAT = "yyyy-MM-dd HH:mm:ss";
        public const string SOLRDATETIMEFORMAT = "yyyy-MM-dd'T'HH:mm:ss'Z'";
        public const int SENTIMENTS_FETCH_PERIOD = 100;
        public const string ANGULAR_DIR = @"..\\FinCore\\ClientApp\\dist";
        public const string API_ROUTE = @"/api";
        public const string API_ROUTE_CONTROLLER = @"/api/[controller]";

        public const short WebBackend_PORT = 2020;
        public const int FAKE_MAGIC_NUMBER = 1000000;
        public const int TOKEN_LIFETIME_HOURS = 18;
        public const int SERVICE_DELAY_MS = 3000;

        public const string JOBGROUP_TECHDETAIL = "Technical Details";
        public const string JOBGROUP_OPENPOSRATIO = "Positions Ratio";
        public const string JOBGROUP_EXECRULES = "Run Rules";
        public const string JOBGROUP_NEWS = "News";
        public const string JOBGROUP_THRIFT = "ThriftServer";
        public const string CRON_MANUAL = "0 0 0 1 1 ? 2100";
        public const string PARAMS_SEPARATOR = "|";
        public const string LIST_SEPARATOR = "~";
        public const string GLOBAL_SECTION_NAME = "Global";
        public const string SETTINGS_PROPERTY_BROKERSERVERTIMEZONE = "BrokerServerTimeZone";
        public const string SETTINGS_PROPERTY_PARSEHISTORY = "NewsEvent.ParseHistory";
        public const string SETTINGS_PROPERTY_STARTHISTORYDATE = "NewsEvent.StartHistoryDate";
        public const string SETTINGS_PROPERTY_USERTIMEZONE = "UserTimeZone";
        public const string SETTINGS_PROPERTY_NETSERVERPORT = "XTrade.NETServerPort";

        public const string SETTINGS_PROPERTY_ENDHISTORYDATE = "NewsEvent.EndHistoryDate";

        // public const string SETTINGS_PROPERTY_THRIFTPORT = "XTrade.ThriftPort";
        public const string SETTINGS_PROPERTY_INSTALLDIR = "InstallDir";
        public const string SETTINGS_PROPERTY_RUNTERMINALUSER = "TerminalUser";
        public const string SETTINGS_PROPERTY_MTCOMMONFILES = "Metatrader.CommonFiles";

        public const string SETTINGS_PROPERTY_MQLSOURCEFOLDER = "MQL.Sources";

        // Max Amount in % you may loose daily
        public const string SETTINGS_PROPERTY_RISK_PER_DAY = "RISK.PERDAY"; // 0.02

        // Mini daily gain that taken into account to do checks losses after gains
        public const string SETTINGS_PROPERTY_RISK_DAILY_MIN_GAIN = "RISK.DAILY_MIN_GAIN"; // 0.0065

        // Losses in % after gains today
        public const string SETTINGS_PROPERY_RISK_DAILY_LOSS_AFTER_GAIN = "RISK.DAILY_LOSS_AFTER_GAIN"; // 0.3
        public const string SETTINGS_PROPERTY_POSITIONS = "POSITIONS";
    }
}