using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using BusinessLogic.Repo;
using BusinessLogic.Repo.Domain;
using BusinessLogic.Scheduler;
using BusinessObjects;
using BusinessObjects.BusinessObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quartz;

namespace BusinessLogic.BusinessObjects
{
    public class MainService : IMainService
    {
        public static MainService thisGlobal;
        private static IWebLog log;
        private SchedulerService _gSchedulerService;
        private TimeZoneInfo BrokerTimeZoneInfo;
        private DataService data;
        private Dictionary<EntitiesEnum, Tuple<Type, Type>> dicTypes;
        private bool Initialized;
        private ConcurrentDictionary<long, ConcurrentQueue<SignalInfo>> signalQue;

        public MainService()
        {
            // RegisterContainer();
            Initialized = false;
            thisGlobal = this;
            isDeploying = false;
            InitTypesMapping();
        }

        public static string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static string MainLogFilePath
        {
            get
            {
                var logfile = "FinCore.MainServer.log";
                return Path.Combine(AssemblyDirectory, logfile);
            }
        }

        private static string RegistryInstallDir
        {
            get
            {
                var result = AssemblyDirectory;
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
                var result = WindowsIdentity.GetCurrent().Name;
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
            var stringBuilder = new StringBuilder();
            var list = data.GetRates(false);
            foreach (var rate in list)
            {
                if (!string.IsNullOrEmpty(rate.Value.Symbol))
                {
                    stringBuilder.Append(rate.Value.Symbol);
                    stringBuilder.Append(',');
                }
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

            data.GetRates(true);

            SetVersion();

            var signal_startServer =
                thisGlobal.CreateSignal(SignalFlags.AllTerminals, 0, EnumSignals.SIGNAL_STARTSERVER, 0);
            PostSignalTo(signal_startServer);

            Initialized = true;
            // doMigration();
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
            var signal_startServer =
                thisGlobal.CreateSignal(SignalFlags.AllTerminals, 0, EnumSignals.SIGNAL_STOPSERVER, 0);
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


        public List<Wallet> GetWalletsState(DateTime date)
        {
            var result = new List<Wallet>();
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

        public List<Wallet> GetWalletBalanceRange(int WID, DateTime fromDate, DateTime toDate)
        {
            var result = new List<Wallet>();
            try
            {
                var dt = fromDate;
                var dateIteration = 3;
                var to = toDate;
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
            var result = false;
            try
            {
                var newws = new DBAccountstate();
                newws.Balance = accState.Balance;
                newws.Date = DateTime.UtcNow;
                var account = new DBAccount();
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

        private void InitTypesMapping()
        {
            dicTypes = new Dictionary<EntitiesEnum, Tuple<Type, Type>>();
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

        protected void SetVersion()
        {
            var date = File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location).ToShortDateString();

            SetGlobalProp("VERSION", "{\"version\": \"" + xtradeConstants.FINCORE_VERSION + " " + date + "\" }");
        }

        private void DoWalletRangeAsync(int WID, DateTime fromDate, DateTime toDate)
        {
            var service = thisGlobal.Container.Resolve<IMessagingService>();
            try
            {
                var dt = fromDate;
                var dateIteration = 3;
                var to = toDate;
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

        /*
        private void RegisterContainer()
        {
            var builder = new ContainerBuilder();
            Container = builder.Build();
        }
        */

        protected TimeZoneInfo GetTimeZoneFromString(string propName)
        {
            var strTimeZone = GetGlobalProp(propName);
            var tz = TimeZoneInfo.GetSystemTimeZones();
            foreach (var tzi in tz)
                if (tzi.StandardName.Equals(strTimeZone))
                    return tzi;
            return null;
        }

        public Wallet CalculateBalanceForDate(int walletId, DateTime dt)
        {
            IList<Wallet> result = data.GetWalletsState(dt);
            var count = result.Count;
            if (count > 0)
            {
                if (walletId != 0) return result.Where(x => x.Id == walletId).FirstOrDefault();

                var wb = new Wallet();
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
            var advisers = new List<DBAdviser>();
            try
            {
                IQueryable<DBAdviser> res = null;
                using (var Session = ConnectionHelper.CreateNewSession())
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
            var advisers = new List<Adviser>();
            try
            {
                using (var Session = ConnectionHelper.CreateNewSession())
                {
                    var adv = Session.Query<DBAdviser>()
                        .Where(x => x.Symbol.Metasymbol.Id == (int) masterId);
                    if (adv == null || advisers.Count() == 0)
                        return advisers; // it is not master expert
                    foreach (var adviser in adv)
                    {
                        var a = new Adviser();
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


        public ExpertInfo InitExpert(ExpertInfo expert)
        {
            try
            {
                using (var Session = ConnectionHelper.CreateNewSession())
                {
                    var msg = "";
                    var accNumber = long.Parse(expert.Account);
                    var terminal = data.getTerminalByNumber(Session, accNumber);
                    if (terminal == null)
                    {
                        msg = $"Unknown AccountNumber {expert.Account}. Please Register Account in DB.";
                        log.Log(msg);
                        expert.Magic = 0;
                        expert.Reason = msg;
                        return expert;
                    }

                    var strSymbol = expert.Symbol;
                    if (strSymbol.Contains("_i"))
                        strSymbol = strSymbol.Substring(0, strSymbol.Length - 2);
                    var symbol = data.getSymbolByName(strSymbol);
                    if (symbol == null)
                    {
                        msg = $"Unknown Symbol {strSymbol}. Please register Symbol in DB.";
                        log.Log(msg);
                        expert.Reason = msg;
                        return expert;
                    }

                    var adviser = data.getAdviser(Session, terminal.Id, symbol.Id, expert.EAName);
                    if (adviser == null)
                    {
                        adviser = new DBAdviser();
                        adviser.Name = expert.EAName;
                        adviser.Timeframe = expert.ChartTimeFrame;
                        adviser.Retired = false;
                        var dbt = new DBTerminal();
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

                    var dynProps = data.GetPropertiesInstance((short) EntitiesEnum.Adviser, adviser.Id);
                    if (dynProps == null || Utils.HasAny(dynProps.Vals) == false)
                    {
                        expert.Data = "";
                    }
                    else
                    {
                        var res = JsonConvert.DeserializeObject<Dictionary<string, DynamicProperty>>(dynProps.Vals);
                        var state = DefaultProperties.transformProperties(res);
                        expert.Data = JsonConvert.SerializeObject(state);
                    }
                    LoadSavedOrders(adviser, ref expert);
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
                using (var Session = ConnectionHelper.CreateNewSession())
                {
                    var accNumber = long.Parse(expert.Account);
                    var terminal = data.getTerminalByNumber(Session, accNumber);
                    if (terminal == null)
                    {
                        var msg = $"Unknown AccountNumber {expert.Account} ERROR";
                        log.Log(msg);
                        expert.Magic = accNumber;
                        expert.Reason = msg;
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
                var accNumber = long.Parse(expert.Account);
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
                case EnumSignals.SIGNAL_POST_LOG:
                {
                    if (signal.Data == null)
                        break;
                    DoLog(signal);
                    result = signal;
                }
                    break;
                case EnumSignals.SIGNAL_INIT_EXPERT:
                    if (signal.Data != null)
                    {
                        var ei = JsonConvert.DeserializeObject<ExpertInfo>(signal.Data.ToString());
                        var expertInfo = InitExpert(ei);
                        result = CreateSignal(SignalFlags.Expert, signal.ObjectId, (EnumSignals) signal.Id,
                            signal.ChartId);

                        result.SetData(JsonConvert.SerializeObject(expertInfo));
                    }

                    break;
                case EnumSignals.SIGNAL_INIT_TERMINAL:
                    if (signal.Data != null)
                    {
                        var ei = JsonConvert.DeserializeObject<ExpertInfo>(signal.Data.ToString());
                        var expertInfo = InitTerminal(ei);
                        result = CreateSignal(SignalFlags.Expert, signal.ObjectId, (EnumSignals) signal.Id,
                            signal.ChartId);
                        result.SetData(JsonConvert.SerializeObject(expertInfo));
                    }

                    break;
                case EnumSignals.SIGNAL_TODAYS_STAT:
                {
                    var ds = Container.Resolve<ITerminalEvents>();
                    if (ds == null)
                        break;
                    result = CreateSignal(SignalFlags.Expert, signal.ObjectId, (EnumSignals) signal.Id, signal.ChartId);
                    var stat = ds.GetTodayStat();
                    result.SetData(JsonConvert.SerializeObject(stat));
                }
                    break;
                case EnumSignals.SIGNAL_CHECK_TRADEALLOWED:
                {
                    var ds = Container.Resolve<ITerminalEvents>();
                    if (ds == null && signal.Data == null)
                        break;
                    var info = Utils.ExtractList<BalanceInfo>(signal.Data);
                    if (Utils.HasAny(info))
                    {
                        BalanceInfo balanceInfo = info.FirstOrDefault();
                        signal.SetData(balanceInfo.ToString());
                        ds.GetTodayStat();
                        ds.CheckTradeAllowed(signal);
                        result = signal;
                    }
                }
                    break;
                case EnumSignals.SIGNAL_GETMAINLOGPATH:
                {
                    var path = GetGlobalProp(xtradeConstants.SETTINGS_PROPERTY_INSTALLDIR);
                    //var di = new DirectoryInfo(path);
                    //if (!di.Exists)
                    //    path = MainLogFilePath;
                    path = Path.Combine(path, "FinCore.MainServer.log");
                    signal.SetData(path);
                    result = signal;
                }
                    break;
                case EnumSignals.SIGNAL_LEVELS4SYMBOL:
                {
                    var symbol = signal.Data.ToString();
                    var levelsString = Levels4Symbol(symbol);
                    result = CreateSignal(SignalFlags.Expert, signal.ObjectId, (EnumSignals) signal.Id, signal.ChartId);
                    result.SetData(levelsString);
                }
                    break;
            }

            return result;
        }

        public string Levels4Symbol(string strSymbol)
        {
            var result = "";
            try
            {
                using (var Session = ConnectionHelper.CreateNewSession())
                {
                    var dbSymbol = data.getSymbolByName(strSymbol);
                    if (dbSymbol == null)
                    {
                        log.Error("ERROR. Unknown Symbol: " + strSymbol + " .");
                        return "";
                    }

                    var metaSymbol = dbSymbol.Metasymbol;
                    if (metaSymbol == null)
                    {
                        log.Error("ERROR. Metasymbol is null for symbol " + strSymbol + " .");
                        return "";
                    }

                    var props = data.GetPropertiesInstance((short) EntitiesEnum.MetaSymbol, metaSymbol.Id);
                    if (!string.IsNullOrEmpty(props.Vals) && props.Vals.Contains("Level"))
                    {
                        var propsList = JsonConvert.DeserializeObject<Dictionary<string, DynamicProperty>>(props.Vals);
                        if (propsList.Any()) return propsList["Levels"].value;
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e);
            }

            return result;
        }

        public string SaveLevels4Symbol(string strSymbol, string levels)
        {
            var result = "";
            try
            {
                using (var Session = ConnectionHelper.CreateNewSession())
                {
                    var dbSymbol = data.getSymbolByName(strSymbol);
                    if (dbSymbol == null)
                    {
                        log.Error("ERROR. Unknown Symbol: " + strSymbol + " .");
                        return "";
                    }

                    var metaSymbol = dbSymbol.Metasymbol;
                    if (metaSymbol == null)
                    {
                        log.Error("ERROR. Metasymbol is null for symbol " + strSymbol + " .");
                        return "";
                    }

                    var props = data.GetPropertiesInstance((short) EntitiesEnum.MetaSymbol, metaSymbol.Id);
                    var propsList = JsonConvert.DeserializeObject<Dictionary<string, DynamicProperty>>(props.Vals);
                    if (propsList.Any())
                    {
                        if (propsList.ContainsKey("Levels"))
                        {
                            propsList["Levels"].value = levels;
                        }
                        else
                        {
                            var newProp = new DynamicProperty();
                            newProp.type = "string";
                            newProp.value = levels;
                            propsList.Add("Levels", newProp);
                        }

                        props.Vals = JsonConvert.SerializeObject(propsList);
                        data.SavePropertiesInstance(props);
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e);
            }

            return result;
        }
        
        private void LoadSavedOrders(DBAdviser adviser, ref ExpertInfo expert)
        {
            var terminals = Container.Resolve<ITerminalEvents>();
            List<PositionInfo> positions = terminals.GetPositions4Adviser(adviser.Id);
            if (Utils.HasAny(positions))
                expert.Orders = JsonConvert.SerializeObject(positions);
        }
        
        public void DeInitExpert(ExpertInfo expert)
        {
            try
            {
                using (var Session = ConnectionHelper.CreateNewSession())
                {
                    var magicNumber = (int) expert.Magic;
                    var adviser = data.getAdviserByMagicNumber(Session, magicNumber);
                    if (adviser == null)
                    {
                        log.Error("Expert with MagicNumber=" + magicNumber + " doesn't exist");
                        return;
                    }

                    UnSubscribeFromSignals(magicNumber);

                    adviser.Running = false;

                    data.SaveInsertAdviser(Session, adviser);
                    var infoMsg = $"Expert On <{adviser.Symbol.Name}> Closed";
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
                var terminals = (List<object>) data.GetObjects(EntitiesEnum.Terminal);
                foreach (var term in terminals)
                {
                    var terminal = (Terminal) term;
                    if (terminal.Retired)
                        continue;
                    var sourceDir = new DirectoryInfo(sourceFolder);
                    var fileName = $@"deployto_{terminal.AccountNumber}.bat";
                    var SW = new StreamWriter(fileName);
                    SW.Write(ProcessFolder("", terminal, sourceFolder, Utils.CopyFile));
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
                var terminal = (Terminal) data.GetObject(EntitiesEnum.Terminal, id);
                if (terminal != null)
                {
                    var pi = new ProcessImpersonation(log);
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
                var message = "Application already deploying: Skip...";
                log.Error(message);
                return message;
            }

            try
            {
                isDeploying = true;
                var terminal = (Terminal) data.GetObject(EntitiesEnum.Terminal, id);
                if (terminal != null)
                {
                    var installDir = GetGlobalProp(xtradeConstants.SETTINGS_PROPERTY_INSTALLDIR);
                    var pi = new ProcessImpersonation(log);
                    var fileName = string.Format(@"{0}\deployto_{1}.bat", installDir, terminal.AccountNumber);
                    var logFile = string.Format(@"{0}\deployto_{1}.log", installDir, terminal.AccountNumber);
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


        public string CompileFile(string folder, Terminal terminal, string file, string targetFolder)
        {
            var ext = Path.GetExtension(file);
            if (ext.Contains("mq5") || ext.Contains("mq4"))
            {
                var compilerApp = "\\metaeditor";
                if (Utils.IsMQL5(targetFolder))
                    compilerApp += "64.exe";
                else
                    compilerApp += ".exe";

                var compilerPath = Path.GetDirectoryName(terminal.FullPath) + compilerApp;
                var targetFile = terminal.CodeBase + folder + "\\" + Path.GetFileName(file);
                return string.Format(@"""{0}"" /compile:""{1}"" {2}", compilerPath, targetFile, Environment.NewLine);
            }

            return "";
        }

        public string ProcessFolder(string folder, Terminal terminal, string sourceFolder, DeployFunc func)
        {
            var result = "";
            var currentSourceFolder = sourceFolder;
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
                        var subF = folder + "\\" + Path.GetFileName(file);
                        result += ProcessFolder(subF, terminal, sourceFolder, func);
                    }

                var files = Directory.EnumerateFiles(currentSourceFolder);
                foreach (var file in files)
                    if (File.Exists(file))
                    {
                        var targetFolder = terminal.CodeBase + folder;
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
            var que = new ConcurrentQueue<SignalInfo>();
            if (!signalQue.ContainsKey(objectId))
            {
                if (signalQue.TryAdd(objectId, que))
                    log.Log($"Object {objectId} added to signals que.");
                else
                    log.Log($"Object {objectId} already existed in signals que.");
            }
        }

        public void UnSubscribeFromSignals(long objectId)
        {
            var que = new ConcurrentQueue<SignalInfo>();
            signalQue.TryRemove(objectId, out que);
        }

        protected void PostSignal(SignalInfo signal)
        {
            ConcurrentQueue<SignalInfo> que;
            if (signalQue.TryGetValue(signal.ObjectId, out que))
                que.Enqueue(signal);
        }

        public void PostSignalTo(SignalInfo signal)
        {
            var to = (SignalFlags) signal.Flags;
            if (to == SignalFlags.AllTerminals)
            {
                foreach (var que in signalQue) que.Value.Enqueue(signal);
            }
            else if (to == SignalFlags.Expert || to == SignalFlags.Terminal)
            {
                PostSignal(signal);
            }
            else if (to == SignalFlags.Server)
            {
                var handler = Container.Resolve<ISignalHandler>();
                handler?.PostSignal(signal, null);
            }
            else if (to == SignalFlags.Cluster)
            {
                var advisers = GetAdvisersByMetaSymbolId(signal.ObjectId);
                if (Utils.HasAny(advisers))
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
                var que = signalQue[ObjectId];
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
            ds?.UpdateBalance(AccountNumber, Balance, Equity);
        }

        public bool UpdateAdviser(Adviser adviser)
        {
            var result = data.UpdateObject(EntitiesEnum.Adviser, adviser.Id, JsonConvert.SerializeObject(adviser));
            var signal = CreateSignal(SignalFlags.Expert, adviser.Id, EnumSignals.SIGNAL_UPDATE_EXPERT, 0);
            signal.Value = result;
            PostSignalTo(signal);
            return result > 0;
        }

        public SignalInfo CreateSignal(SignalFlags flags, long ObjectId, EnumSignals Id, long chartId)
        {
            var signal = new SignalInfo();
            signal.Flags = (long) flags;
            signal.Id = (int) Id;
            signal.ObjectId = ObjectId;
            signal.Value = 1;
            signal.ChartId = chartId;
            return signal;
        }

        public void DoLog(SignalInfo signal)
        {
            var message = new StringBuilder();
            message.Append("<" + signal.ObjectId + ">:");
            message.Append("(" + signal.Sym + "):");
            message.Append(signal.Data);
            log.Log(message.ToString());
            log.Info(message);
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
            if (!Utils.HasAny(rates))
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

        protected static List<LogItem> LogItems = new List<LogItem>();

        public object LogList()
        {
            if (LogItems == null) LogItems = new List<LogItem>();
            if (Utils.HasAny(LogItems))
                LogItems.Clear();

            var item0 = new LogItem();
            item0.Name = "log0";
            item0.TabTitle = "UI Log";
            item0.DataSource = "";
            item0.TextChangedEvent = "selectTab($event)";
            item0.Theme = "ace/theme/terminal";
            LogItems.Add(item0);

            var item1 = new LogItem();
            item1.Name = "log1";
            item1.TabTitle = "FinCore Log";
            item1.DataSource = "";
            item1.TextChangedEvent = "";
            item1.Theme = "ace/theme/solarized_dark";
            item1.Path = MainLogFilePath;
            LogItems.Add(item1);

            try
            {
                var i = 2;
                var terminals = (List<object>) data.GetObjects(EntitiesEnum.Terminal);
                foreach (var term in terminals)
                {
                    var terminal = (Terminal) term;
                    if (terminal.Retired)
                        continue;
                    // add expert log
                    var itemx = new LogItem();
                    itemx.Name = $"log{i}";
                    itemx.TabTitle = terminal.Broker + " Expert";
                    itemx.DataSource = "";
                    itemx.TextChangedEvent = "";
                    itemx.Theme = "ace/theme/solarized_light";

                    var directory = new DirectoryInfo(terminal.CodeBase + "\\Logs");
                    if (directory.Exists)
                    {
                        if (Utils.HasAny(directory.GetFiles()))
                        {
                            var logFile = (from f in directory.GetFiles()
                                orderby f.LastWriteTime descending
                                select f).First();
                            if (logFile.Exists)
                                itemx.Path = logFile.FullName; //  terminal.CodeBase + "/Logs";
                        }
                    }

                    LogItems.Add(itemx);
                    i++;


                    // Add sys log
                    var itemy = new LogItem();
                    itemy.Name = $"log{i}";
                    itemy.TabTitle = terminal.Broker + " System";
                    itemy.DataSource = "";
                    itemy.TextChangedEvent = "";
                    itemy.Theme = "ace/theme/github";

                    var directory2 = new DirectoryInfo(terminal.CodeBase);
                    if (directory2.Exists)
                    {
                        directory2 = new DirectoryInfo(directory2.Parent.FullName + "/logs");
                        if (Utils.HasAny(directory2.GetFiles()))
                        {
                            var logFile2 = (from f in directory2.GetFiles()
                                orderby f.LastAccessTime descending
                                select f).First();
                            if (logFile2.Exists)
                                itemy.Path = logFile2.FullName;
                        }
                    }

                    LogItems.Add(itemy);
                    i++;
                }
            }
            catch (Exception e)
            {
                log.Info("Failed to get Metatrader Logs: " + e); 
            }

            return LogItems;
        }

        public string GetLogContent(string logName, long numberOfLines)
        {
            var result = "";
            try
            {
                if (!Utils.HasAny(LogItems))
                    return "No Logs Found";
                var logitems = LogItems.Where(x => x.Name.CompareTo(logName) == 0);
                if (!Utils.HasAny(logitems))
                    return $"No log file with key name found: {logName}";
                var logPath = logitems.FirstOrDefault().Path;
                if (string.IsNullOrEmpty(logPath))
                    return "";
                if (!new FileInfo(logPath).Exists)
                    return $"Log file with path {logPath} doesn't exists";

                using (var fs = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var reader = new StreamReader(fs);
                    reader.BaseStream.Seek(0, SeekOrigin.End);
                    var totallines = numberOfLines <= 0 ? 1000000 : numberOfLines;
                    var count = 0;
                    while (count < totallines && reader.BaseStream.Position > 0)
                    {
                        reader.BaseStream.Position--;
                        var c = reader.BaseStream.ReadByte();
                        if (reader.BaseStream.Position > 0)
                            reader.BaseStream.Position--;
                        if (c == Convert.ToInt32('\n')) ++count;
                    }

                    result = reader.ReadToEnd();
                    //string[] arr = str.Replace("\r", "").Split('\n');
                    reader.Close();
                }

                return result;
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        #endregion
    }
}