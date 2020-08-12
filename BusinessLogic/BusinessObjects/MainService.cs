using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Autofac;
using BusinessObjects;
using BusinessLogic.Repo;
using BusinessLogic.Scheduler;
using NHibernate;
using NHibernate.Type;
using Quartz;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BusinessObjects.BusinessObjects;
using System.Threading.Tasks;

namespace BusinessLogic.BusinessObjects
{
    public class MainService : IMainService
    {
        public const int CHAR_BUFF_SIZE = 512;
        public static MainService thisGlobal;
        private static int isDebug = -1;
        public static char[] ParamsSeparator = xtradeConstants.PARAMS_SEPARATOR.ToCharArray();
        protected static IWebLog log;
        private SchedulerService _gSchedulerService;
        protected TimeZoneInfo BrokerTimeZoneInfo;
        private DataService data;
        private bool Initialized;
        private ConcurrentDictionary<long, ConcurrentQueue<SignalInfo>> signalQue;
        private Dictionary<EntitiesEnum, Tuple<Type, Type> > dicTypes;

        public MainService()
        {
            // RegisterContainer();
            Initialized = false;
            thisGlobal = this;
            isDeploying = false;
            InitTypesMapping();
        }

        private void InitTypesMapping()
        {
            dicTypes = new Dictionary<EntitiesEnum, Tuple<Type, Type> >();
            TupleMap<DBAccount, Account>(EntitiesEnum.Account);
            TupleMap<DBTerminal, Terminal>(EntitiesEnum.Terminal);
            TupleMap<DBMetasymbol, MetaSymbol>(EntitiesEnum.MetaSymbol);
            TupleMap<DBWallet, Wallet>(EntitiesEnum.Wallet);
            TupleMap<DBJobs, ScheduledJobView>(EntitiesEnum.Jobs);
            TupleMap<DBDeals, DealInfo>(EntitiesEnum.Deals);
            TupleMap<DBRates, Rates>(EntitiesEnum.Rates);
            TupleMap<DBSymbol, Symbol>(EntitiesEnum.Symbol);
            TupleMap<DBAccountstate, AccountState>(EntitiesEnum.AccountState);
            TupleMap<DBAdviser, Adviser>(EntitiesEnum.Adviser);
            TupleMap<DBSettings, Settings>(EntitiesEnum.Settings);
            TupleMap<DBPerson, Person>(EntitiesEnum.Person);
        }

        private void TupleMap<DBT, DTO>(EntitiesEnum t) 
        {
            dicTypes[t] = new Tuple<Type, Type>(typeof(DBT), typeof(DTO));
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static string RegistryInstallDir
        {
            get
            {
                string result = AssemblyDirectory;
                try
                {
                    result = thisGlobal.GetGlobalProp(xtradeConstants.SETTINGS_PROPERTY_INSTALLDIR);
                }
                catch (Exception e)
                {
                    log.Error(e);
                }

                return result;
            }
        }

        public static string MTTerminalUserName
        {
            get
            {
                string result = WindowsIdentity.GetCurrent().Name;
                try
                {
                    result = thisGlobal.GetGlobalProp(xtradeConstants.SETTINGS_PROPERTY_RUNTERMINALUSER);
                    //RegistryKey rk = Registry.LocalMachine.OpenSubKey(xtradeConstants.SETTINGS_APPREGKEY, false);
                    //if (rk == null)
                    //{
                    //    rk = Registry.LocalMachine.CreateSubKey(xtradeConstants.SETTINGS_APPREGKEY, true, RegistryOptions.None);
                    //    rk.SetValue("MTTerminalUserName", result);
                    //}
                    //else
                    //{
                    //    result = rk.GetValue("RunMTTerminalUserName")?.ToString();
                    //}
                }
                catch (Exception e)
                {
                    log.Error(e);
                }

                return result;
            }
        }

        public Tuple<Type, Type> EnumToType(EntitiesEnum entities)
        {
            if (!dicTypes.ContainsKey(entities))
                return null;
            return dicTypes[entities];
        }

        public Dictionary<EntitiesEnum, Tuple<Type, Type>> GetTypes()
        {
            return dicTypes;
        }

        public List<CurrencyInfo> GetCurrencies()
        {
            return data.GetCurrencies();
        }

        public ConcurrentDictionary<string, Rates> GetRates(bool IsReread)
        {
            return data.GetRates(IsReread);
        }

        public string GetRatesList()
        {
            StringBuilder stringBuilder = new StringBuilder();
            var list = data.GetRates(false);
            foreach( var rate in list)
            {
                stringBuilder.Append(rate.Key);
                stringBuilder.Append(",");
            }
            return stringBuilder.ToString();
        }

        public decimal ConvertToUSD(decimal value, string valueCurrency)
        {
            return data.ConvertToUSD(value, valueCurrency);
        }

        public ILifetimeScope Container { get; private set; }

        public Person LoginPerson(string mail, string password)
        {
            return data.LoginPerson(mail, password);
        }

        public void Init(ILifetimeScope container)
        {
            if (Initialized)
                return;
            Container = container;
            signalQue = new ConcurrentDictionary<long, ConcurrentQueue<SignalInfo>>();

            data = Container.Resolve<DataService>();

            log = thisGlobal.Container.Resolve<IWebLog>();

            log.Info("InstallDir: " + RegistryInstallDir);

            BrokerTimeZoneInfo = GetBrokerTimeZone();

            InitScheduler(true);
            SetVersion();

            SignalInfo signal_startServer = MainService.thisGlobal.CreateSignal(SignalFlags.AllTerminals, 0, EnumSignals.SIGNAL_STARTSERVER);
            PostSignalTo(signal_startServer);

            Initialized = true;
            // doMigration();
        }

        protected void SetVersion()
        {
            string date = File.GetLastWriteTime((Assembly.GetExecutingAssembly().Location)).ToShortDateString();

            SetGlobalProp("VERSION", "{\"version\": \"0.9.0 " + date + "\" }");
        }

        /*protected void doMigration()
        {
            log.Info("Start Migration: Adviser");
            data.MigrateAdvisers();
            log.Info("End Migration: Adviser");
        }*/

        public bool InitScheduler(bool serverMode /*unused*/)
        {
            if (_gSchedulerService == null)
                _gSchedulerService = Container.Resolve<SchedulerService>();
            return _gSchedulerService.Initialize();
        }

        public TimeZoneInfo GetBrokerTimeZone()
        {
            if (BrokerTimeZoneInfo == null)
                BrokerTimeZoneInfo = GetTimeZoneFromString(xtradeConstants.SETTINGS_PROPERTY_BROKERSERVERTIMEZONE);

            return BrokerTimeZoneInfo;
        }

        public string GetGlobalProp(string name)
        {
            return data.GetGlobalProp(name);
        }

        public void SetGlobalProp(string name, string value)
        {
            data.SetGlobalProp(name, value);
        }

        public List<ScheduledJobInfo> GetAllJobsList()
        {
            return SchedulerService.GetAllJobsList();
        }

        public Dictionary<string, ScheduledJobInfo> GetRunningJobs()
        {
            return SchedulerService.GetRunningJobs();
        }

        public DateTime? GetJobNextTime(string group, string name)
        {
            return SchedulerService.GetJobNextTime(group, name);
        }

        public DateTime? GetJobPrevTime(string group, string name)
        {
            return SchedulerService.GetJobPrevTime(group, name);
        }

        public void Dispose()
        {
            SignalInfo signal_startServer = MainService.thisGlobal.CreateSignal(SignalFlags.AllTerminals, 0, EnumSignals.SIGNAL_STOPSERVER);
            PostSignalTo(signal_startServer);

            if (_gSchedulerService != null)
                _gSchedulerService.Shutdown();
           
            // To enable QUIK: uncomment these lines
            //ITerminalConnector connector = thisGlobal.Container.Resolve<ITerminalConnector>();
            //if (connector != null)
            //    connector.Dispose();
        }

        public void PauseScheduler()
        {
            SchedulerService.sched.PauseAll();
        }

        public void ResumeScheduler()
        {
            SchedulerService.sched.ResumeAll();
        }

        public bool IsDebug()
        {
            if (isDebug == -1)
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                bool isDebugBuild = assembly.GetCustomAttributes(false).OfType<DebuggableAttribute>()
                    .Select(attr => attr.IsJITTrackingEnabled).FirstOrDefault();
                if (isDebugBuild)
                {
                    isDebug = 1;
                    return true;
                }

                isDebug = 0;
                return false;
            }

            return isDebug > 0 ? true : false;
        }

        public List<Wallet> GetWalletsState(DateTime date)
        {
            List<Wallet> result = new List<Wallet>();
            try
            {
                result = data.GetWalletsState(date);
            }
            catch (Exception e)
            {
                log.Error("Error: GetWalletsState: " + e);
            }

            return result;
        }

        public void GetWalletBalanceRangeAsync(int WalletId, DateTime from, DateTime to)
        {
            var task = new Task(() => DoWalletRangeAsync(WalletId, from, to));
            task.Start();
        }

        private void DoWalletRangeAsync(int WID, DateTime fromDate, DateTime toDate)
        {
            var service = MainService.thisGlobal.Container.Resolve<IMessagingService>();
            try
            {
                DateTime dt = fromDate;
                int dateIteration = 3;
                DateTime to = toDate;
                Wallet res = null;
                while (dt <= to)
                {
                    res = CalculateBalanceForDate(WID, dt);
                    // result.Add(res);
                    service.SendMessage(WsMessageType.ChartValue, res);
                    dt = dt.AddDays(dateIteration);
                }

                res = CalculateBalanceForDate(WID, toDate);
                service.SendMessage(WsMessageType.ChartValue, res);
            }
            catch (Exception e)
            {
                log.Error("Error: GetWalletBalanceRange: " + e);
            }
            service.SendMessage(WsMessageType.ChartDone, "");
        }

        public List<Wallet> GetWalletBalanceRange(int WID, DateTime fromDate, DateTime toDate)
        {
            List<Wallet> result = new List<Wallet>();
            try
            {
                DateTime dt = fromDate;
                int dateIteration = 3;
                DateTime to = toDate;
                while (dt <= to)
                {
                    var res = CalculateBalanceForDate(WID, dt);
                    result.Add(res);
                    dt = dt.AddDays(dateIteration);
                }

                result.Add(CalculateBalanceForDate(WID, toDate));
            }
            catch (Exception e)
            {
                log.Error("Error: GetWalletBalanceRange: " + e);
            }

            return result;
        }

        public bool UpdateAccountState(AccountState accState)
        {
            bool result = false;
            try
            {
                DBAccountstate newws = new DBAccountstate();
                newws.Balance = accState.Balance;
                newws.Date = DateTime.UtcNow;
                DBAccount account = new DBAccount();
                account.Id = accState.AccountId;
                newws.Account = account;
                newws.Comment = accState.Comment;
                data.SaveInsertWaletState(newws);
                return true;
            }
            catch (Exception e)
            {
                log.Error("Error: UpdateAccountState: " + e);
            }

            return result;
        }

        /*
        private void RegisterContainer()
        {
            var builder = new ContainerBuilder();
            Container = builder.Build();
        }
        */

        protected TimeZoneInfo GetTimeZoneFromString(string propName)
        {
            string strTimeZone = GetGlobalProp(propName);
            ReadOnlyCollection<TimeZoneInfo> tz = TimeZoneInfo.GetSystemTimeZones();
            foreach (TimeZoneInfo tzi in tz)
                if (tzi.StandardName.Equals(strTimeZone))
                    return tzi;
            return null;
        }

        public Wallet CalculateBalanceForDate(int walletId, DateTime dt)
        {
            IList<Wallet> result = data.GetWalletsState(dt);
            int count = result.Count;
            if (count > 0)
            {
                if (walletId != 0) return result.Where(x => x.Id == walletId).FirstOrDefault();

                Wallet wb = new Wallet();
                wb.Id = walletId;
                if (dt.Equals(DateTime.MaxValue))
                    wb.Date = DateTime.UtcNow;
                else
                    wb.Date = dt;
                foreach (var row in result)
                    wb.Balance += row.Balance; // for total
                //wb.PersonId = row.PersonId;
                //wb.Retired = row.Retired;
                //wb.Name = row.Name;

                return wb;
            }

            return null;
        }

        public IEnumerable<DBAdviser> GetAdvisersByTerminal(long terminalId)
        {
            List<DBAdviser> advisers = new List<DBAdviser>();
            try
            {
                IQueryable<DBAdviser> res = null;
                using (ISession Session = ConnectionHelper.CreateNewSession())
                {
                    res = Session.Query<DBAdviser>().Where(x => x.Terminal.Id == terminalId && x.Retired == false);
                    foreach (var adviser in res)
                        advisers.Add(adviser);
                }

                return advisers;
            }
            catch (Exception e)
            {
                log.Error("Error: GetAdvisersByTerminal: " + e);
            }

            return advisers;
        }

        public IEnumerable<Adviser> GetAdvisersByMetaSymbolId(long masterId)
        {
            List<Adviser> advisers = new List<Adviser>();
            try
            {
                using (ISession Session = ConnectionHelper.CreateNewSession())
                {
                    var adv = Session.Query<DBAdviser>()
                        .Where(x => x.Symbol.Metasymbol.Id == (int) masterId);
                    if (adv == null || advisers.Count() == 0)
                        return advisers; // it is not master expert
                    foreach (var adviser in adv)
                    {
                        Adviser a = new Adviser();
                        if (ExpertsRepository.toDTO(adviser, ref a))
                            advisers.Add(a);
                    }
                }

                return advisers;
            }
            catch (Exception e)
            {
                log.Error("Error: GetAdvisersByMetaSymbolId: " + e);
            }

            return advisers;
        }


        #region DBJobs

        public void UnsheduleJobs(IEnumerable<JobKey> jobs)
        {
            foreach (var job in jobs)
                SchedulerService.removeJobTriggers(job);
        }

        public bool DeleteJob(JobKey job)
        {
            return SchedulerService.sched.DeleteJob(job).Result;
        }

        #endregion

        #region Jobs

        public void RunJobNow(string group, string name)
        {
            SchedulerService.RunJobNow(new JobKey(name, group));
        }

        public void StopJobNow(string group, string name)
        {
            SchedulerService.StopJobNow(new JobKey(name, group));
        }

        public string GetJobProp(string group, string name, string prop)
        {
            return SchedulerService.GetJobProp(group, name, prop);
        }

        public void SetJobCronSchedule(string group, string name, string cron)
        {
            SchedulerService.SetJobCronSchedule(group, name, cron);
        }

        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(int Section, string Key,
            string Value, [MarshalAs(UnmanagedType.LPArray)] byte[] Result,
            int Size, string FileName);

        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileStringW", SetLastError = true,
            CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileStringW(string lpApplicationName, string lpKeyName, string lpDefault,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)]
            char[] lpReturnedString, int nSize, string Filename);

        [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileStringW", CharSet = CharSet.Unicode)]
        private static extern int WritePrivateProfileStringW(string lpApplicationName, int lpKeyName, int lpString,
            string lpFileName);

        [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileStringW", CharSet = CharSet.Unicode)]
        private static extern int WritePrivateProfileStringW2(string lpApplicationName, string lpKeyName,
            string lpString,
            string lpFileName);

        // The Function called to obtain the SectionHeaders,
        // and returns them in an Dynamic Array.
        public string[] GetSectionNames(string path)
        {
            try
            {
                //    Sets the maxsize buffer to 500, if the more
                //    is required then doubles the size each time.
                for (int maxsize = CHAR_BUFF_SIZE;; maxsize *= 2)
                {
                    //    Obtains the information in bytes and stores
                    //    them in the maxsize buffer (Bytes array)
                    byte[] bytes = new byte[maxsize];
                    int size = GetPrivateProfileString(0, "", "", bytes, maxsize, path);

                    // Check the information obtained is not bigger
                    // than the allocated maxsize buffer - 2 bytes.
                    // if it is, then skip over the next section
                    // so that the maxsize buffer can be doubled.
                    if (size < maxsize - 2)
                    {
                        // Converts the bytes value into an ASCII char. This is one long string.
                        string Selected = Encoding.ASCII.GetString(bytes, 0,
                            size - (size > 0 ? 1 : 0));
                        // Splits the Long string into an array based on the "\0"
                        // or null (Newline) value and returns the value(s) in an array
                        return Selected.Split('\0');
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("Failed to get Section Names from file: " + path + ". Error: " + e);
            }

            return new[] {""};
        }

        public static string GetPrivateProfileString(string fileName, string sectionName, string keyName)
        {
            char[] ret = new char[CHAR_BUFF_SIZE];

            while (true)
            {
                int length = GetPrivateProfileStringW(sectionName, keyName, null, ret, ret.Length, fileName);
                if (length == 0)
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

                // This function behaves differently if both sectionName and keyName are null
                if (sectionName != null && keyName != null)
                {
                    if (length == ret.Length - 1)
                        ret = new char[ret.Length * 2];
                    else
                        return new string(ret, 0, length);
                }
                else
                {
                    if (length == ret.Length - 2)
                        ret = new char[ret.Length * 2];
                    else
                        return new string(ret, 0, length - 1);
                }
            }
        }

        public ExpertInfo InitExpert(ExpertInfo expert)
        {
            try
            {
                using (ISession Session = ConnectionHelper.CreateNewSession())
                {
                    long accNumber = long.Parse(expert.Account);
                    Terminal terminal = data.getTerminalByNumber(Session, accNumber);
                    if (terminal == null)
                    {
                        log.Log("Unknown AccountNumber " + expert.Account + " ERROR");
                        expert.Magic = 0;
                        return expert;
                    }

                    string strSymbol = expert.Symbol;
                    if (strSymbol.Contains("_i"))
                        strSymbol = strSymbol.Substring(0, strSymbol.Length - 2);
                    DBSymbol symbol = data.getSymbolByName(strSymbol);
                    if (symbol == null)
                    {
                        log.Log("Unknown Symbol " + strSymbol + " ERROR");
                        return expert;
                    }

                    DBAdviser adviser = data.getAdviser(Session, terminal.Id, symbol.Id, expert.EAName);
                    if (adviser == null)
                    {
                        adviser = new DBAdviser();
                        adviser.Name = expert.EAName;
                        adviser.Timeframe = expert.ChartTimeFrame;
                        adviser.Retired = false;
                        DBTerminal dbt = new DBTerminal();
                        dbt.Id = terminal.Id;
                        adviser.Terminal = dbt;
                        adviser.Symbol = symbol;
                    }
                    else
                    {
                        expert.IsMaster = adviser.IsMaster;
                    }

                    adviser.Running = true;
                    if (adviser.Id <= 0)
                        data.SaveInsertAdviser(Session, adviser);

                    var dynProps = this.data.GetPropertiesInstance((short)EntitiesEnum.Adviser, adviser.Id);
                    if ((dynProps == null) || (dynProps.Vals == null) || (dynProps.Vals.Length == 0))
                    {
                        expert.Data = "";
                    }
                    else
                    {
                        Dictionary<string, DynamicProperty> res = JsonConvert.DeserializeObject<Dictionary<string, DynamicProperty>>(dynProps.Vals);
                        var state = DefaultProperties.transformProperties(res);
                        expert.Data = JsonConvert.SerializeObject(state);
                    }
                    //if (!string.IsNullOrEmpty(adviser.State))
                    //    expert.Data = adviser.State;

                    GetOrdersListToLoad(adviser, ref expert);
                    expert.Magic = adviser.Id;
                    SubscribeToSignals(adviser.Id);

                    log.Info($"Expert On <{adviser.Symbol.Name}> On TF=<{expert.ChartTimeFrame}> loaded successfully!");
                }

                return expert;
            }
            catch (Exception e)
            {
                log.Error(e);
                expert.Magic = 0;
            }

            return expert;
        }

        public ExpertInfo InitTerminal(ExpertInfo expert)
        {
            try
            {
                using (ISession Session = ConnectionHelper.CreateNewSession())
                {
                    long accNumber = long.Parse(expert.Account);
                    Terminal terminal = data.getTerminalByNumber(Session, accNumber);
                    if (terminal == null)
                    {
                        log.Log("Unknown AccountNumber " + expert.Account + " ERROR");
                        expert.Magic = accNumber;
                        return expert;
                    }
                    SubscribeToSignals(accNumber);
                    log.Info($"Terminal <{accNumber}> Subscribed.");
                }
                return expert;
            }
            catch (Exception e)
            {
                log.Error(e);
                expert.Magic = 0;
            }
            return expert;
        }

        public void DeInitTerminal(ExpertInfo expert)
        {
            try
            {
                long accNumber = long.Parse(expert.Account);
                UnSubscribeFromSignals(accNumber);
                log.Info($"Terminal: <{accNumber}> UnSubscribed.");
            }
            catch (Exception e)
            {
                log.Error("DeInitTerminal: " + e);
            }
        }

        public SignalInfo SendSignal(SignalInfo signal)
        {
            SignalInfo result = null;
            switch ((EnumSignals) signal.Id)
            {
                case EnumSignals.SIGNAL_INIT_EXPERT:
                    if (signal.Data != null)
                    {
                        ExpertInfo ei = JsonConvert.DeserializeObject<ExpertInfo>(signal.Data.ToString());
                        var expertInfo = InitExpert(ei);
                        result = CreateSignal(SignalFlags.Expert, signal.ObjectId, (EnumSignals) signal.Id);

                        result.Data = JsonConvert.SerializeObject(expertInfo);
                    }
                    break;
                case EnumSignals.SIGNAL_INIT_TERMINAL:
                    if (signal.Data != null)
                    {
                        ExpertInfo ei = JsonConvert.DeserializeObject<ExpertInfo>(signal.Data.ToString());
                        var expertInfo = InitTerminal(ei);
                        result = CreateSignal(SignalFlags.Expert, signal.ObjectId, (EnumSignals)signal.Id);
                        result.Data = JsonConvert.SerializeObject(expertInfo);
                    }
                    break;
                case EnumSignals.SIGNAL_TODAYS_STAT:
                    {
                        var ds = Container.Resolve<ITerminalEvents>();
                        if (ds == null)
                            break;
                        result = CreateSignal(SignalFlags.Expert, signal.ObjectId, (EnumSignals)signal.Id);
                        var stat = ds.GetTodayStat();
                        result.Data = JsonConvert.SerializeObject(stat);
                    }
                    break;
                case EnumSignals.SIGNAL_CHECK_TRADEALLOWED:
                    {
                        var ds = Container.Resolve<ITerminalEvents>();
                        if ((ds == null) && (signal.Data == null))
                            break;
                        JArray arr = signal.Data as JArray;
                        if ((arr == null) || (arr.Count <= 0))
                            break;
                        string data = arr.FirstOrDefault().ToString();
                        Dictionary<string, string> paramsList = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
                        signal.Data = paramsList["Balance"];
                        ds.GetTodayStat();
                        ds.CheckTradeAllowed(signal);
                        result = signal;
                    }
                    break;
                case EnumSignals.SIGNAL_LEVELS4SYMBOL:
                    {
                        string symbol = signal.Data.ToString();
                        string levelsString = Levels4Symbol(symbol);
                        result = CreateSignal(SignalFlags.Expert, signal.ObjectId, (EnumSignals)signal.Id);
                        result.Data = levelsString;
                    }
                    break;
            }
            return result;
        }
        private string Levels4Symbol(string strSymbol)
        {
            string result = "";
            try
            {
                using (ISession Session = ConnectionHelper.CreateNewSession())
                {
                    DBSymbol dbSymbol = data.getSymbolByName(strSymbol);
                    if (dbSymbol == null)
                    {
                        log.Error("ERROR. Unknown Symbol: " + strSymbol + " .");
                        return "";
                    }
                    DBMetasymbol metaSymbol = dbSymbol.Metasymbol;
                    if (metaSymbol == null)
                    {
                        log.Error("ERROR. Metasymbol is null for symbol " + strSymbol + " .");
                        return "";
                    }
                    DynamicProperties props = data.GetPropertiesInstance((short)EntitiesEnum.MetaSymbol, metaSymbol.Id);
                    if (!String.IsNullOrEmpty(props.Vals) && props.Vals.Contains("Level"))
                    {
                        Dictionary<string, DynamicProperty> propsList = JsonConvert.DeserializeObject<Dictionary<string, DynamicProperty>>(props.Vals);
                        if (propsList.Any())
                        {
                            return propsList["Levels"].value;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e);
            }
            return result;
        }

        private bool GetOrdersListToLoad(DBAdviser adviser, ref ExpertInfo expert)
        {
            //if (expert.OrderTicketsToLoad == null)
            //    expert.OrderTicketsToLoad = new List<string>();
            /*
            List<PositionInfo> positions = new List<PositionInfo>();
            string filePath = GetAdviserFilePath(adviser);
            if (!File.Exists(filePath))
                return false;
            string[] sections = GetSectionNames(filePath);
            if (sections.Length > 0)
            {
                List<string> sectionsList = sections.ToList();
                if ((sectionsList != null) && (sectionsList.Count() > 0))
                {
                    foreach (var order in sectionsList)
                    {
                        if (order.Equals(xtradeConstants.GLOBAL_SECTION_NAME) )
                            continue;
                        string roleString = GetPrivateProfileString(filePath, order, "role");
                        if (!String.IsNullOrEmpty(roleString))
                        {
                            ENUM_ORDERROLE role = (ENUM_ORDERROLE)Int32.Parse(roleString);
                            if (role != ENUM_ORDERROLE.History)
                            {
                                //string ticketString = GetPrivateProfileString(filePath, order, "ticket");
                                expert.OrderTicketsToLoad.Add(order);
                            }
                        }
                        //DeleteSection(order, filePath);
                        //if (!order.Equals(xtradeConstants.GLOBAL_SECTION_NAME))
                        //    WritePrivateProfileStringW(order, 0, 0, filePath);
                    }
                }
            }
            return true;
            */
            return false;
        }

        /* public void SaveExpert(ExpertInfo expert)
        {
            try
            {
                int magicNumber = (int) expert.Magic;
                using (ISession Session = ConnectionHelper.CreateNewSession())
                {
                    DBAdviser adviser = data.getAdviserByMagicNumber(Session, magicNumber);
                    if (adviser == null)
                    {
                        log.Log("Expert with Magic=" + magicNumber + " doesn't exist");
                        return;
                    }

                    adviser.Running = true;
                    adviser.Lastupdate = DateTime.UtcNow;
                    if (!string.IsNullOrEmpty(expert.Data))
                        if (string.IsNullOrEmpty(adviser.State)) // || (expert.Data.CompareTo(adviser.State) != 0)
                            adviser.State = expert.Data;

                    var terminals = Container.Resolve<ITerminalEvents>();
                    if (terminals != null)
                    {
                        long accountNumber = long.Parse(expert.Account);
                        List<PositionInfo> positions = null;
                        if (!String.IsNullOrEmpty(expert.Orders))
                            positions = JsonConvert.DeserializeObject<List<PositionInfo>>(expert.Orders);
                        else
                            positions = new List<PositionInfo>();
                        terminals.UpdateSLTP(magicNumber, accountNumber, positions);
                    }


                    data.SaveInsertAdviser(Session, adviser);
                }
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }*/

        public int DeleteHistoryOrders(string filePath)
        {
            int result = 0;
            string[] sections = GetSectionNames(filePath);
            if (sections.Length > 0)
            {
                List<string> sectionsList = sections.ToList();
                if (sectionsList != null && sectionsList.Count() > 0)
                    foreach (var sectionName in sectionsList)
                    {
                        if (sectionName.Equals(xtradeConstants.GLOBAL_SECTION_NAME))
                            continue;

                        string roleString = GetPrivateProfileString(filePath, sectionName, "role");
                        if (!string.IsNullOrEmpty(roleString))
                        {
                            ENUM_ORDERROLE role = (ENUM_ORDERROLE) int.Parse(roleString);
                            if (role.Equals(ENUM_ORDERROLE.History))
                            {
                                // Deletes section!!!
                                WritePrivateProfileStringW(sectionName, 0, 0, filePath);
                                result++;
                            }
                        }
                    }
            }

            return result;
        }

        protected string GetAdviserFilePath(DBAdviser adviser)
        {
            string path = GetGlobalProp(xtradeConstants.SETTINGS_PROPERTY_MTCOMMONFILES);
            string sym = adviser.Symbol.Name;
            //if (sym.Length > 6)
            //    sym = sym.Remove(3, 1);
            string filePath = $"{path}\\{adviser.Terminal.Accountnumber}_{sym}_{adviser.Timeframe}_{adviser.Id}.set";
            return filePath;
        }

        public bool FileLocked(string FileName)
        {
            FileStream fs = null;

            try
            {
                // NOTE: This doesn't handle situations where file is opened for writing by another process but put into write shared mode, it will not throw an exception and won't show it as write locked
                fs = File.Open(FileName, FileMode.Open, FileAccess.ReadWrite,
                    FileShare.None); // If we can't open file for reading and writing then it's locked by another process for writing
            }
            catch (UnauthorizedAccessException) // https://msdn.microsoft.com/en-us/library/y973b725(v=vs.110).aspx
            {
                // This is because the file is Read-Only and we tried to open in ReadWrite mode, now try to open in Read only mode
                try
                {
                    fs = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.None);
                }
                catch (Exception)
                {
                    return true; // This file has been locked, we can't even open it to read
                }
            }
            catch (Exception)
            {
                return true; // This file has been locked
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }

            return false;
        }

        private string ReasonToString(int Reason)
        {
            switch (Reason)
            {
                case 0: //0
                    return
                        "0 <REASON_PROGRAM> - Expert Advisor terminated its operation by calling the _ExpertRemove()_ function";
                case 1: //1
                    return "1 <REASON_REMOVE> Program has been deleted from the chart";
                case 2: // 2
                    return "2 <REASON_RECOMPILE> Program has been recompiled";
                case 3: //3
                    return "3 <REASON_CHARTCHANGE> Symbol or chart period has been changed";
                case 4:
                    return "4 <REASON_CHARTCLOSE> Chart has been closed";
                case 5:
                    return "5 <REASON_PARAMETERS> Input parameters have been changed by a user";
                case 6:
                    return
                        "6 <REASON_ACCOUNT> Another account has been activated or reconnection to the trade server has occurred due to changes in the account settings";
                case 7:
                    return "7 <REASON_TEMPLATE> A new template has been applied";
                case 8:
                    return
                        "8 <REASON_INITFAILED> This value means that _OnInit()_ handler has returned a nonzero value";
                case 9:
                    return "9 <REASON_CLOSE> Terminal has been closed";
            }

            return $"Unknown reason: {Reason}";
        }

        public void DeInitExpert(ExpertInfo expert)
        {
            try
            {
                using (ISession Session = ConnectionHelper.CreateNewSession())
                {
                    int magicNumber = (int) expert.Magic;
                    DBAdviser adviser = data.getAdviserByMagicNumber(Session, magicNumber);
                    if (adviser == null)
                    {
                        log.Error("Expert with MagicNumber=" + magicNumber + " doesn't exist");
                        return;
                    }

                    UnSubscribeFromSignals(magicNumber);

                    adviser.Running = false;

                    data.SaveInsertAdviser(Session, adviser);
                    string infoMsg = $"Expert On <{adviser.Symbol.Name}> Closed";
                    log.Info(infoMsg); // with reason {ReasonToString(expert.Reason)}.
                }
            }
            catch (Exception e)
            {
                log.Error("DeInitExpert: " + e);
            }
        }

        public void DeployToTerminals(string sourceFolder)
        {
            try
            {
                var terminals = (List<Terminal>)data.GetObjects(EntitiesEnum.Terminal);
                foreach (var terminal in terminals)
                {
                    DirectoryInfo sourceDir = new DirectoryInfo(sourceFolder);
                    string fileName = string.Format(@"deployto_{0}.bat", terminal.AccountNumber);
                    StreamWriter SW = new StreamWriter(fileName);
                    SW.Write(ProcessFolder("", terminal, sourceFolder, CopyFile));
                    SW.Write(ProcessFolder("", terminal, sourceFolder, CompileFile));
                    SW.Flush();
                    SW.Close();
                    SW.Dispose();
                    SW = null;
                    //Process.Start("deploy.bat");
                }
            }
            catch (Exception e)
            {
                log.Error("Error: Generate Deploy Scripts: " + e);
            }
        }

        protected void CloseTerminal(int id)
        {
            try
            {
                var terminal = (Terminal)data.GetObject(EntitiesEnum.Terminal, id);
                if (terminal != null)
                {
                    ProcessImpersonation pi = new ProcessImpersonation(log);
                    pi.CloseTerminal(terminal.FullPath);
                }
            }
            catch (Exception e)
            {
                log.Info("CloseTerminal error: " + e);
            }
        }

        protected bool isDeploying;

        public string DeployToAccount(int id)
        {
            if (isDeploying)
            {
                string message = "Application already deploying: Skip...";
                log.Error(message);
                return message;
            }

            try
            {
                isDeploying = true;
                var terminal = (Terminal)data.GetObject(EntitiesEnum.Terminal, id);
                if (terminal != null)
                {
                    string installDir = GetGlobalProp(xtradeConstants.SETTINGS_PROPERTY_INSTALLDIR);
                    ProcessImpersonation pi = new ProcessImpersonation(log);
                    string fileName = string.Format(@"{0}\deployto_{1}.bat", installDir, terminal.AccountNumber);
                    string logFile = string.Format(@"{0}\deployto_{1}.log", installDir, terminal.AccountNumber);
                    CloseTerminal(terminal.Id);
                    pi.StartProcessInNewThread(fileName, logFile, terminal.FullPath);
                    return $"Deploy process started for terminal {terminal.AccountNumber}!";
                }
                else
                {
                    return $"Terminal with ID={id} not found or disabled!!!";
                }
            }
            catch (Exception e)
            {
                var message = "Error: DeployToAccount: " + e;
                log.Error(message);
                return message;
            }
            finally
            {
                isDeploying = false;
            }
        }

        public delegate string DeployFunc(string folder, Terminal terminal, string file, string targetFolder);

        public string CopyFile(string folder, Terminal terminal, string file, string targetFolder)
        {
            return string.Format(@"xcopy /y {0} {1}{2}", file, targetFolder, Environment.NewLine);
        }

        public bool IsMQL5(string path)
        {
            return path.Contains("MQL5");
        }

        public string CompileFile(string folder, Terminal terminal, string file, string targetFolder)
        {
            string ext = Path.GetExtension(file);
            if (ext.Contains("mq5") || ext.Contains("mq4"))
            {
                string compilerApp = "\\metaeditor";
                if (IsMQL5(targetFolder))
                    compilerApp += "64.exe";
                else
                    compilerApp += ".exe";

                string compilerPath = Path.GetDirectoryName(terminal.FullPath) + compilerApp;
                string targetFile = terminal.CodeBase + folder + "\\" + Path.GetFileName(file);
                return string.Format(@"""{0}"" /compile:""{1}"" {2}", compilerPath, targetFile, Environment.NewLine);
            }

            return "";
        }

        public string ProcessFolder(string folder, Terminal terminal, string sourceFolder, DeployFunc func)
        {
            string result = "";
            string currentSourceFolder = sourceFolder;
            if (folder.Length > 0)
                currentSourceFolder += folder;
            if (!Directory.Exists(currentSourceFolder))
                return result;
            try
            {
                var folders = Directory.EnumerateDirectories(currentSourceFolder);
                foreach (var file in folders)
                    if (Directory.Exists(file))
                    {
                        string subF = folder + "\\" + Path.GetFileName(file);
                        result += ProcessFolder(subF, terminal, sourceFolder, func);
                    }

                var files = Directory.EnumerateFiles(currentSourceFolder);
                foreach (var file in files)
                    if (File.Exists(file))
                    {
                        string targetFolder = terminal.CodeBase + folder;
                        // process file
                        result += func(folder, terminal, file, targetFolder);
                        // result += string.Format(@"xcopy /y {0} {1}{2}", file, targetFolder, Environment.NewLine);
                    }
            }
            catch (Exception e)
            {
                log.Info(e.ToString());
            }

            return result;
        }


        public void SubscribeToSignals(long objectId)
        {
            ConcurrentQueue<SignalInfo> que = new ConcurrentQueue<SignalInfo>();
            //signalQue.AddOrUpdate(objectId, que, (oldkey, oldvalue) => que);
            if (signalQue.TryAdd(objectId, que))
                log.Log($"Object {objectId} added to signals que.");
            else
                log.Log($"Object {objectId} already existed in signals que.");

        }

        public void UnSubscribeFromSignals(long objectId)
        {
            ConcurrentQueue<SignalInfo> que = new ConcurrentQueue<SignalInfo>();
            signalQue.TryRemove(objectId, out que);
        }

        protected void PostSignal(SignalInfo signal)
        {
            if (signalQue.ContainsKey(signal.ObjectId))
            {
                ConcurrentQueue<SignalInfo> que = signalQue[signal.ObjectId];
                que.Enqueue(signal);
            }
        }

        public void PostSignalTo(SignalInfo signal)
        {
            SignalFlags to = (SignalFlags) signal.Flags;
            if (to == SignalFlags.AllTerminals)
            {
                foreach (var que in signalQue) que.Value.Enqueue(signal);
            }
            else if ((to == SignalFlags.Expert) || (to == SignalFlags.Terminal))
            {
                PostSignal(signal);
            }
            else if (to == SignalFlags.Server)
            {
                ISignalHandler handler = Container.Resolve<ISignalHandler>();
                if (handler != null)
                    handler.PostSignal(signal);
            }
            else if (to == SignalFlags.Cluster)
            {
                var advisers = GetAdvisersByMetaSymbolId(signal.ObjectId);
                if (advisers != null && advisers.Count() > 0)
                    foreach (var adv in advisers)
                    {
                        signal.Flags = (long) SignalFlags.Expert;
                        signal.ObjectId = adv.Id;
                        PostSignal(signal);
                    }
            }
        }

        public SignalInfo ListenSignal(long ObjectId, long flags)
        {
            if (signalQue.ContainsKey(ObjectId))
            {
                ConcurrentQueue<SignalInfo> que = signalQue[ObjectId];
                if (que.Count > 0)
                {
                    SignalInfo si;
                    if (que.TryDequeue(out si)) return si;
                }
            }
            return null;
        }

        public void UpdateBalance(int AccountNumber, decimal Balance, decimal Equity)
        {
            data.UpdateBalance(AccountNumber, Balance, Equity);
            var ds = Container.Resolve<ITerminalEvents>();
            if (ds != null)
                ds.UpdateBalance(AccountNumber, Balance, Equity);
        }

        public bool UpdateAdviser(Adviser adviser)
        {
            int result = data.UpdateObject(EntitiesEnum.Adviser, adviser.Id, JsonConvert.SerializeObject(adviser));
            SignalInfo signal = CreateSignal(SignalFlags.Expert, adviser.Id, EnumSignals.SIGNAL_UPDATE_EXPERT);
            signal.Value = result;
            PostSignalTo(signal);
            return result > 0;
        }

        public SignalInfo CreateSignal(SignalFlags flags, long ObjectId, EnumSignals Id)
        {
            SignalInfo signal = new SignalInfo();
            signal.Flags = (long) flags;
            signal.Id = (int) Id;
            signal.Name = Id.ToString();
            signal.ObjectId = ObjectId;
            signal.Value = 1;
            signal.RaiseDateTime = DateTime.Now.ToString(xtradeConstants.MTDATETIMEFORMAT);
            return signal;
        }

        public void SaveDeals(List<DealInfo> deals)
        {
            data.SaveDeals(deals);
        }

        public List<DealInfo> GetDeals()
        {
            return data.GetDeals();
        }


        public List<DealInfo> TodayDeals()
        {
            return data.TodayDeals();
        }

        public void UpdateRates(List<RatesInfo> rates)
        {
            if ((rates == null) || (rates.Count <= 0))
                return;
            data.UpdateRates(rates);
        }

        public object GetObjects(EntitiesEnum type)
        {
            return data.GetObjects(type);
        }

        public object GetChildObjects(EntitiesEnum parentType, EntitiesEnum childType, int parentKey)
        {
            return data.GetChildObjects(parentType, childType, parentKey);
        }

        public object GetObject(EntitiesEnum type, int id)
        {
            return data.GetObject(type, id);
        }

        public int InsertObject(EntitiesEnum type, string values)
        {
            return data.InsertObject(type, values);
        }

        public int UpdateObject(EntitiesEnum type, int id, string values)
        {
            return data.UpdateObject(type, id, values);
        }

        public int DeleteObject(EntitiesEnum type, int id)
        {
            return data.DeleteObject(type, id);
        }
        #endregion

 
    }
}