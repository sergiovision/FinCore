using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using AutoMapper;
using BusinessLogic.BusinessObjects;
using BusinessLogic.Repo.Domain;
using BusinessObjects;
using BusinessObjects.BusinessObjects;
using Newtonsoft.Json;
using NHibernate;
using NHibernate.Type;

namespace BusinessLogic.Repo;

public class DataService : IDataService
{
    private static IWebLog log;
    private static readonly object lockDeals = new object();
    private readonly IRepository<DBAccountstate> accstates;
    private readonly IRepository<DBCurrency> currencies;
    private readonly ExpertsRepository experts;
    private readonly IRepository<DBJobs> jobs;
    private readonly IRepository<DBMetasymbol> metaSymbols;
    private readonly AuthRepository persons;
    private readonly IRepository<DBProperties> props;
    private readonly IRepository<DBSettings> settings;
    private readonly IRepository<DBSymbol> symbols;
    private readonly WalletsRepository wallets;
    private IRepository<DBDeals> deals;
    private readonly IMapper mapper;
    private RatesService rates;

    public DataService(IWebLog l)
    {
        symbols = new BaseRepository<DBSymbol>();
        
        currencies = new BaseRepository<DBCurrency>();
        settings = new BaseRepository<DBSettings>();
        jobs = new BaseRepository<DBJobs>();
        accstates = new BaseRepository<DBAccountstate>();
        experts = new ExpertsRepository();
        persons = new AuthRepository();
        deals = new BaseRepository<DBDeals>();
        props = new BaseRepository<DBProperties>();
        metaSymbols = new BaseRepository<DBMetasymbol>();
        log = l;
        mapper = MainService.thisGlobal.Container.Resolve<IMapper>();
        rates = MainService.thisGlobal.Container.Resolve<RatesService>();
        wallets = new WalletsRepository(rates);
#if DEBUG
        mapper.ConfigurationProvider.AssertConfigurationIsValid();
#endif
        
    }

    public List<CurrencyInfo> GetCurrencies()
    {
        var result = new List<CurrencyInfo>();
        try
        {
            currencies.GetAll().ForEach(currency =>
            {
                var curr = new CurrencyInfo();
                curr.Id = (short) currency.Id;
                curr.Name = currency.Name;
                curr.Retired = currency.Enabled.Value > 0 ? false : true;
                result.Add(curr);
            });
        }
        catch (Exception e)
        {
            log.Error("Error: GetCurrencies: " + e);
        }

        return result;
    }

    public string GetGlobalProp(string name)
    {
        try
        {
            var gvars = settings.GetAll().Where(x => x.Propertyname.Equals(name));
            if (gvars.Count() > 0)
            {
                var gvar = gvars.First();
                return gvar.Value;
            }
        }
        catch (Exception e)
        {
            log.Error("Error: GetGlobalProp: " + e);
        }

        return "";
    }

    public void SetGlobalProp(string name, string value)
    {
        var gvars = settings.GetAll().Where(x => x.Propertyname == name);
        if (gvars.Count() > 0)
        {
            var gvar = gvars.First();
            gvar.Value = value;
            settings.Update(gvar);
        }
        else
        {
            var gvar = new DBSettings();
            gvar.Propertyname = name;
            gvar.Value = value;
            settings.Insert(gvar);
        }
    }


    public List<DealInfo> TodayDeals()
    {
        var result = new List<DealInfo>();
        try
        {
            var now = DateTime.Now;
            using (var Session = ConnectionHelper.CreateNewSession())
            {
                var deals = Session.Query<DBDeals>().OrderByDescending(x => x.Closetime);
                foreach (var dbd in deals)
                    if (Utils.IsSameDay(dbd.Closetime.Value, now))
                        result.Add(toDTO(dbd));
            }
        }
        catch (Exception e)
        {
            log.Error("Error: TodayDeals: " + e);
        }

        return result;
    }

    public void UpdateBalance(int AccountNumber, decimal Balance, decimal Equity)
    {
        using (var Session = ConnectionHelper.CreateNewSession())
        {
            var terms = Session.Query<DBTerminal>().Where(x => x.Accountnumber == AccountNumber);
            if (!Utils.HasAny(terms))
                return;
            var terminal = terms.FirstOrDefault();
            if (terminal == null)
                return;
            if (terminal.Account == null)
                return;
            using (var Transaction = Session.BeginTransaction())
            {
                terminal.Account.Balance = Balance;
                terminal.Account.Equity = Equity;
                terminal.Account.Lastupdate = DateTime.UtcNow;
                Session.Update(terminal);
                Transaction.Commit();
            }

            var acc = Session.Query<DBAccountstate>().Where(x => x.Account.Id == terminal.Account.Id)
                .OrderByDescending(x => x.Date);
            using (var Transaction = Session.BeginTransaction())
            {
                if (acc.Any())
                {
                    DBAccountstate state = null;
                    state = acc.FirstOrDefault();
                    if (state == null || state.Date.DayOfYear != DateTime.Today.DayOfYear)
                    {
                        var newstate = new DBAccountstate();
                        if (state == null)
                            newstate.Account = terminal.Account;
                        else
                            newstate.Account = state.Account;
                        newstate.Balance = Balance;
                        newstate.Comment = "Autoupdate";
                        newstate.Date = DateTime.UtcNow;
                        Session.Save(newstate);
                    }
                    else
                    {
                        state.Balance = Balance;
                        state.Comment = "Autoupdate";
                        state.Date = DateTime.UtcNow;
                        Session.Update(state);
                    }

                    Transaction.Commit();
                }
            }
        }
    }

    /*
    public void MigrateAdvisers()
    {
        try
        {
            experts.MigrateAdvisersData();
        }
        catch (Exception e)
        {
            log.Error("Error: MigrateAdvisers: " + e);
        }

    }*/


    public List<Wallet> GetWalletsState(DateTime date, bool showRetired)
    {
        var result = new List<Wallet>();
        try
        {
            return wallets.GetWalletsState(date, showRetired);
        }
        catch (Exception e)
        {
            log.Error("Error: GetCurrentWalletsState: " + e);
        }

        return result;
    }

    public void SaveDeals(List<DealInfo> deals)
    {
        if (!Utils.HasAny(deals))
            return;
        try
        {
            lock (lockDeals)
            {
                var i = 0;
                using (var Session = ConnectionHelper.CreateNewSession())
                {
                    foreach (var deal in deals.OrderBy(x => x.CloseTime))
                    {
                        var sym = getSymbolByName(deal.Symbol);
                        if (sym == null)
                            continue;
                        var dbDeal = Session.Get<DBDeals>((int) deal.Ticket);
                        if (dbDeal == null)
                        {
                            if (getDealById(Session, deal.Ticket) != null)
                                continue;
                            try
                            {
                                using (var Transaction = Session.BeginTransaction())
                                {
                                    dbDeal = new DBDeals();
                                    dbDeal.Dealid = (int) deal.Ticket;
                                    dbDeal.Symbol = getSymbolByName(deal.Symbol);
                                    dbDeal.Terminal = getBDTerminalByNumber(Session, deal.Account);
                                    dbDeal.Id = (int) deal.Ticket;
                                    DateTime closeTime;
                                    if (DateTime.TryParse(deal.CloseTime, out closeTime))
                                        dbDeal.Closetime = DateTime.Parse(deal.CloseTime);
                                    dbDeal.Comment = deal.Comment;
                                    dbDeal.Commission = (decimal) deal.Commission;
                                    DateTime openTime;
                                    if (DateTime.TryParse(deal.OpenTime, out openTime))
                                        dbDeal.Opentime = DateTime.Parse(deal.OpenTime);
                                    dbDeal.Orderid = (int) deal.OrderId;
                                    dbDeal.Profit = (decimal) deal.Profit;
                                    dbDeal.Price = (decimal) deal.ClosePrice;
                                    dbDeal.Swap = (decimal) deal.Swap;
                                    dbDeal.Typ = deal.Type;
                                    dbDeal.Volume = (decimal) deal.Lots;
                                    Session.Save(dbDeal);
                                    Transaction.Commit();
                                    i++;
                                }
                            }
                            catch (Exception)
                            {
                                log.Log($"Deal {deal.Ticket}:{deal.Symbol} failed to be saved in database");
                            }
                        }
                    }
                }

                if (i > 0)
                {
                    log.Log($"Saved {i} history deals in database");
                }
            }
        }
        catch (Exception e)
        {
            var message = "Error: DataService.SaveDeals: " + e;
            log.Error(message);
            log.Log(message);
        }
    }
    
    public List<Asset> AssetsDistribution(int type)
    {
        return wallets.AssetsDistribution(type);
    }

    public IEnumerable<MetaSymbolStat> MetaSymbolStatistics(int count, int option)
    {
        var result = new List<MetaSymbolStat>();
        try
        {
            using (var Session = ConnectionHelper.CreateNewSession())
            {
                List<DBMetasymbol> symbols = null;
                if (option == 0)
                    symbols = Session.Query<DBMetasymbol>().Where(x => x.Retired == false).ToList();
                else if (option == 1)
                {
                    string listSymbolsString = GetGlobalProp(xtradeConstants.SETTINGS_METASYMBOLS_STATISTICS);
                    string[] listMetaSymbols = new List<string>(listSymbolsString.Split(',')).ToArray();
                    symbols = Session.Query<DBMetasymbol>().Where(x => x.Retired == false && listMetaSymbols.Contains(x.Name)).ToList();
                }
                foreach (var sym in symbols)
                {
                    var deals = Session.Query<DBDeals>().Where(x => x.Symbol.Metasymbol.Id == sym.Id);
                    decimal sumProfit = 0;
                    var countTrades = 0;
                    DateTime tradeDate = DateTime.MinValue;
                    foreach (var deal in deals)
                    {
                        string currency = "USD";
                        if (deal.Terminal != null)
                            if ( deal.Terminal.Account != null)
                                currency = deal.Terminal.Account.Currency.Name;
                        sumProfit += rates.ConvertToUSD(deal.Profit, currency);
                        countTrades++;
                        if (deal.Closetime.HasValue)
                        {
                            if (tradeDate < deal.Closetime.Value)
                                tradeDate = deal.Closetime.Value;
                        }
                        else
                        {
                            if (tradeDate < deal.Opentime)
                                tradeDate = deal.Opentime;
                        }
                    }
                    if (countTrades <= 10)
                        continue;
                    var mss = new MetaSymbolStat();
                    mss.MetaId = sym.Id;
                    mss.Name = sym.Name;
                    mss.Description = sym.Description;
                    mss.TotalProfit = sumProfit;
                    mss.NumOfTrades = countTrades;
                    mss.ProfitPerTrade = sumProfit / countTrades;
                    mss.Date = tradeDate;
                    result.Add(mss);
                }
            }
        }
        catch (Exception e)
        {
            log.Error("Error in MetaSymbolStat : " + e);
        }
        var moreResult = result.OrderByDescending(x => x.Date).ThenByDescending(y=>y.ProfitPerTrade);
        if (count > 1)
        {
            return moreResult.Take(count).OrderByDescending(x => x.TotalProfit);
        }
        return moreResult;
    }

    public void StartPerf(int month)
    {
        var task = new Task(() => PerfAsync(month));
        task.Start();
    }

    private void PerfAsync(int month)
    {
        var service = MainService.thisGlobal.Container.Resolve<IMessagingService>();
        try
        {
            using (var Session = ConnectionHelper.CreateNewSession())
            {
                var rateList = Session.Query<DBRates>().Where(x => x.Retired == false).ToList();
                var now = DateTime.Now;
                var dayFrom = 1;
                var year = now.Year;
                if (now.Month < month + 1) year--;

                var from = new DateTime(year, month + 1, dayFrom);
                var dayTo = now.Month == month + 1 ? now.Day : DateTime.DaysInMonth(year, month + 1);
                var to = new DateTime(year, month + 1, dayTo);
                var Accounts = Session.Query<DBAccount>(); 
                for (var i = dayFrom; i <= dayTo; i++)
                {
                    var forDate = new DateTime(year, month + 1, i);
                    var forDateEnd = new DateTime(year, month + 1, i, 23, 50, 0);
                    var ts = new TimeStat();
                    ts.X = i;
                    ts.Date = forDate;
                    ts.Period = TimePeriod.Daily;
                    ts.Gains = 0;
                    ts.Losses = 0;
                    foreach (var acc in Accounts)
                    {
                        var accStateAll = Session.Query<DBAccountstate>().Where(x => x.Account.Id == acc.Id);
                        var accResultsStart = accStateAll.Where(x => x.Date <= forDate)
                            .OrderByDescending(x => x.Date);
                        var accResultsEnd = accStateAll.Where(x => x.Date <= forDateEnd)
                            .OrderByDescending(x => x.Date);

                        if (accResultsEnd == null || accResultsEnd.Count() == 0)
                            continue;
                        if (accResultsStart == null || accResultsStart.Count() == 0)
                            continue;

                        var accStateEnd = accResultsEnd.FirstOrDefault();
                        var balanceStart = new decimal(0);
                        var balanceEnd = new decimal(0);
                        if (accStateEnd != null)
                        {
                            balanceEnd = rates.ConvertToUSD(accStateEnd.Balance, acc.Currency.Name);
                            if (acc.Typ > 0)
                                ts.InvestingValue += balanceEnd;

                            ts.CheckingValue += balanceEnd;
                        }

                        var accStateStart = accResultsStart.FirstOrDefault();
                        if (accStateStart != null)
                        {
                            balanceStart = rates.ConvertToUSD(accStateStart.Balance, acc.Currency.Name);
                            if (acc.Typ > 0)
                                ts.InvestingChange += balanceStart;

                            ts.CheckingChange += balanceStart;
                        }
                    }

                    ts.CheckingChange = ts.CheckingValue - ts.CheckingChange;
                    ts.InvestingChange = ts.InvestingValue - ts.InvestingChange;
                    if (ts.CheckingChange > 0)
                        ts.Gains = ts.CheckingChange;
                    else
                        ts.Losses = Math.Abs(ts.CheckingChange);

                    ts.CheckingChange = Math.Round(ts.CheckingChange, 2);
                    ts.InvestingChange = Math.Round(ts.InvestingChange, 2);
                    ts.CheckingValue = Math.Round(ts.CheckingValue, 2);
                    ts.InvestingValue = Math.Round(ts.InvestingValue, 2);
                    ts.Losses = Math.Round(ts.Losses, 2);
                    ts.Gains = Math.Round(ts.Gains, 2);

                    service.SendMessage(WsMessageType.ChartValue, ts);
                }
            }
        }
        catch (Exception e)
        {
            log.Error("Error in Performance : " + e);
        }

        service.SendMessage(WsMessageType.ChartDone, "");
    }

    public List<TimeStat> Performance(int month, TimePeriod period)
    {
        var result = new List<TimeStat>();
        try
        {
            using (var Session = ConnectionHelper.CreateNewSession())
            {
                var rateList = Session.Query<DBRates>().Where(x => x.Retired == false).ToList();
                var now = DateTime.Now;
                var dayFrom = 1;
                var year = now.Year;
                if (now.Month < month + 1) year--;

                var from = new DateTime(year, month + 1, dayFrom);
                var dayTo = now.Month == month + 1 ? now.Day : DateTime.DaysInMonth(year, month + 1);
                var to = new DateTime(year, month + 1, dayTo);
                var Accounts = Session.Query<DBAccount>(); // .Where(x => (x.Retired == false));
                for (var i = dayFrom; i <= dayTo; i++)
                {
                    var forDate = new DateTime(year, month + 1, i);
                    var forDateEnd = new DateTime(year, month + 1, i, 23, 50, 0);
                    var ts = new TimeStat();
                    ts.X = i;
                    ts.Date = forDate;
                    ts.Period = period;
                    ts.Gains = 0;
                    ts.Losses = 0;
                    foreach (var acc in Accounts)
                    {
                        var accStateAll = Session.Query<DBAccountstate>().Where(x => x.Account.Id == acc.Id);
                        var accResultsStart = accStateAll.Where(x => x.Date <= forDate)
                            .OrderByDescending(x => x.Date);
                        var accResultsEnd = accStateAll.Where(x => x.Date <= forDateEnd)
                            .OrderByDescending(x => x.Date);

                        if (accResultsEnd == null || accResultsEnd.Count() == 0)
                            continue;
                        if (accResultsStart == null || accResultsStart.Count() == 0)
                            continue;

                        var accStateEnd = accResultsEnd.FirstOrDefault();
                        var balanceStart = new decimal(0);
                        var balanceEnd = new decimal(0);
                        if (accStateEnd != null)
                        {
                            balanceEnd = rates.ConvertToUSD(accStateEnd.Balance, acc.Currency.Name);
                            if (acc.Typ > 0)
                                ts.InvestingValue += balanceEnd;

                            ts.CheckingValue += balanceEnd;
                        }

                        var accStateStart = accResultsStart.FirstOrDefault();
                        if (accStateStart != null)
                        {
                            balanceStart = rates.ConvertToUSD(accStateStart.Balance, acc.Currency.Name);
                            if (acc.Typ > 0)
                                ts.InvestingChange += balanceStart;

                            ts.CheckingChange += balanceStart;
                        }

                        /*if (balanceStart > balanceEnd)
                        {
                            ts.Losses += (balanceStart - balanceEnd);
                        }
                        if (balanceEnd > balanceStart)
                        {
                            ts.Gains += (balanceEnd - balanceStart);
                        }*/
                    }

                    ts.CheckingChange = ts.CheckingValue - ts.CheckingChange;
                    ts.InvestingChange = ts.InvestingValue - ts.InvestingChange;
                    if (ts.CheckingChange > 0)
                        ts.Gains = ts.CheckingChange;
                    else
                        ts.Losses = Math.Abs(ts.CheckingChange);

                    ts.CheckingChange = Math.Round(ts.CheckingChange, 2);
                    ts.InvestingChange = Math.Round(ts.InvestingChange, 2);
                    ts.CheckingValue = Math.Round(ts.CheckingValue, 2);
                    ts.InvestingValue = Math.Round(ts.InvestingValue, 2);
                    ts.Losses = Math.Round(ts.Losses, 2);
                    ts.Gains = Math.Round(ts.Gains, 2);

                    result.Add(ts);
                }
            }
        }
        catch (Exception e)
        {
            log.Error("Error in Performance : " + e);
        }

        return result;
    }

    public List<DealInfo> GetDeals()
    {
        var result = new List<DealInfo>();
        try
        {
            using (var Session = ConnectionHelper.CreateNewSession())
            {
                var deals = Session.Query<DBDeals>().OrderByDescending(x => x.Closetime);
                foreach (var dbd in deals)
                    result.Add(toDTO(dbd));
            }
        }
        catch (Exception e)
        {
            log.Error("Error: GetDeals: " + e);
        }

        return result;
    }


    public static bool toDTO(DBAccount a, ref Account result)
    {
        try
        {
            result.Id = a.Id;
            result.Number = a.Number;
            result.Balance = a.Balance;
            if (a.Terminal != null)
                result.TerminalId = a.Terminal.Id;
            result.Lastupdate = a.Lastupdate;
            result.Description = a.Description;
            result.Equity = a.Equity;
            if (a.Person != null)
                result.PersonId = a.Person.Id;
            if (a.Currency != null)
                result.CurrencyStr = a.Currency.Name;
            result.Retired = a.Retired;
            if (a.Wallet != null)
                result.WalletId = a.Wallet.Id;
            result.Typ = (AccountType) a.Typ;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool toDTO(DBTerminal t, ref Terminal result)
    {
        try
        {
            result.AccountNumber = t.Accountnumber.Value;
            result.Broker = t.Broker;
            result.CodeBase = t.Codebase;
            result.Retired = t.Retired;
            result.FullPath = t.Fullpath;
            result.Stopped = t.Stopped;
            result.Id = t.Id;
            if (t.Account != null)
                result.Currency = t.Account.Currency.Name;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public DealInfo toDTO(DBDeals deal)
    {
        var result = new DealInfo();
        if (deal.Closetime.HasValue)
            result.CloseTime = deal.Closetime.Value.ToString(xtradeConstants.MTDATETIMEFORMAT);
        result.Comment = deal.Comment;
        result.Commission = (double) deal.Commission;
        result.Lots = (double) deal.Volume;

        result.OpenPrice = (double) deal.Price;
        result.OpenTime = deal.Opentime.ToString(xtradeConstants.MTDATETIMEFORMAT);
        result.Profit = (double) deal.Profit;
        if (deal.Terminal != null)
        {
            if (deal.Terminal.Accountnumber.HasValue)
                result.Account = deal.Terminal.Accountnumber.Value;
            result.AccountName = deal.Terminal.Broker;
        }
        result.Swap = (double) deal.Swap;
        if (deal.Symbol != null)
            result.Symbol = deal.Symbol.Name;
        if (deal.Orderid.HasValue)
            result.Ticket = deal.Orderid.Value;
        result.Type = (sbyte) deal.Typ;
        return result;
    }
    
    #region LocalFuncs

    private bool toPropsDTO(DBProperties p, ref DynamicProperties result)
    {
        try
        {
            result.ID = p.ID;
            result.entityType = p.entityType;
            result.objId = p.objId;
            result.Vals = p.Vals;
            result.updated = p.updated;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public IEnumerable<DynamicProperties> GetAllProperties()
    {
        try
        {
            var results = new List<DynamicProperties>();
            var result = props.GetAll();
            if (!result.Any())
                return results;
            result.ForEach(x =>
            {
                var dynProps = new DynamicProperties();
                if (toPropsDTO(x, ref dynProps))
                    results.Add(dynProps);
            });
            return results;
        }
        catch (Exception e)
        {
            log.Error("Error: GetAllProperties: " + e);
        }

        return null;
    }

    public DynamicProperties GetPropertiesInstance(short entityType, int objId)
    {
        try
        {
            var newdP = new DynamicProperties();
            var result = props.GetAll().Where(x => x.entityType == entityType && x.objId == objId);
            if (result.Any())
                if (toPropsDTO(result.FirstOrDefault(), ref newdP))
                    return newdP;
            using (var Session = ConnectionHelper.CreateNewSession())
            {
                using (var Transaction = Session.BeginTransaction())
                {
                    var gvar = new DBProperties();
                    // gvar.ID = newdP.ID; // ID should be Autogenerated by DB
                    gvar.entityType = entityType;
                    gvar.objId = objId;
                    var defProps = new Dictionary<string, DynamicProperty>();
                    defProps = DefaultProperties.fillProperties(ref defProps, (EntitiesEnum) entityType, -1, objId,
                        "");
                    gvar.Vals = JsonConvert.SerializeObject(defProps);
                    gvar.updated = DateTime.UtcNow;
                    Session.Save(gvar);
                    Transaction.Commit();
                    if (toPropsDTO(gvar, ref newdP))
                        return newdP;
                }
            }
        }
        catch (Exception e)
        {
            log.Error("Error: GetPropertiesInstance: " + e);
        }

        return null;
    }

    public bool SavePropertiesInstance(DynamicProperties newdP)
    {
        try
        {
            using (var Session = ConnectionHelper.CreateNewSession())
            {
                using (var Transaction = Session.BeginTransaction())
                {
                    var result = Session.Get<DBProperties>(newdP.ID);
                    if (result != null)
                    {
                        result.entityType = newdP.entityType;
                        result.objId = newdP.objId;
                        result.Vals = newdP.Vals;
                        result.updated = DateTime.UtcNow;
                        Session.Update(result);
                    }
                    else
                    {
                        var gvar = new DBProperties();
                        // gvar.ID = newdP.ID; // ID should be Autogenerated by DB
                        gvar.entityType = newdP.entityType;
                        gvar.objId = newdP.objId;
                        gvar.Vals = newdP.Vals;
                        gvar.updated = DateTime.UtcNow;
                        Session.Save(gvar);
                    }

                    Transaction.Commit();
                    return true;
                }
            }
        }
        catch (Exception e)
        {
            log.Error("Error: UpdateProperties: " + e);
        }

        return false;
    }

    public IEnumerable<DBJobs> GetDBActiveJobsList()
    {
        try
        {
            var result = jobs.GetAll().Where(x => x.Disabled == false);
            return result;
        }
        catch (Exception e)
        {
            log.Error("Error: GetDBActiveJobsList: " + e);
        }

        return null;
    }

    public DBAdviser getAdviserByMagicNumber(ISession Session, long magicNumber)
    {
        try
        {
            var adviser = Session.Get<DBAdviser>((int) magicNumber);
            return adviser;
        }
        catch (Exception e)
        {
            log.Error("Error: getAdviserByMagicNumber: " + e);
        }

        return null;
    }

    public DBCurrency getCurrencyID(string currencyStr)
    {
        try
        {
            var result = currencies.GetAll().Where(x => x.Name.Equals(currencyStr));
            if (result.Any())
                return result.First();
        }
        catch (Exception e)
        {
            log.Error("Error: getAdviserByMagicNumber: " + e);
        }

        return null;
    }

    public Terminal getTerminalByNumber(ISession Session, long AccountNumber)
    {
        try
        {
            var result = Session.Query<DBTerminal>().Where(x => x.Accountnumber == (int) AccountNumber);
            if (result.Any())
            {
                var term = result.FirstOrDefault();
                var terminal = new Terminal();
                if (term != null && toDTO(term, ref terminal))
                    return terminal;
            }
        }
        catch (Exception e)
        {
            log.Error("Error: getTerminalByNumber: " + e);
        }

        return null;
    }

    public DBTerminal getBDTerminalByNumber(ISession Session, long AccountNumber)
    {
        try
        {
            var result = Session.Query<DBTerminal>().Where(x => x.Accountnumber == (int) AccountNumber);
            if (result.Any()) return result.FirstOrDefault();
        }
        catch (Exception e)
        {
            log.Error("Error: getTerminalByNumber: " + e);
        }

        return null;
    }

    public DBDeals getDealById(ISession Session, long DealId)
    {
        try
        {
            var result = Session.Query<DBDeals>().Where(x => x.Dealid == (int) DealId);
            if (result.Any()) return result.FirstOrDefault();
        }
        catch (Exception e)
        {
            log.Error("Error: getDealById: " + e);
        }

        return null;
    }


    public DBSymbol getSymbolByName(string SymbolStr)
    {
        try
        {
            var result = symbols.GetAll().Where(x => x.Name.Equals(SymbolStr));
            if (result.Any())
                return result.First();
        }
        catch (Exception e)
        {
            log.Error("Error: getSymbolByName: " + e);
        }

        return null;
    }

    public DBAdviser getAdviser(ISession Session, int term_id, int sym_id, string ea)
    {
        try
        {
            var result = Session.Query<DBAdviser>().Where(x =>
                x.Terminal.Id == term_id && x.Symbol.Id == sym_id && x.Name == ea && x.Retired == false);
            if (result != null && result.Count() > 0)
                return result.FirstOrDefault();
        }
        catch (Exception e)
        {
            log.Error("Error: getSymbolByName: " + e);
        }

        return null;
    }

    public void SaveInsertAdviser(ISession Session, DBAdviser toAdd)
    {
        using (var Transaction = Session.BeginTransaction())
        {
            if (toAdd.Id == 0)
                Session.Save(toAdd);
            else
                Session.Update(toAdd);

            Transaction.Commit();
        }
    }


    public void SaveInsertWaletState(DBAccountstate toAdd)
    {
        try
        {
            var accState = accstates.Insert(toAdd);
            if (accState == null)
                return;
            using (var Session = ConnectionHelper.CreateNewSession())
            {
                using (var Transaction = Session.BeginTransaction())
                {
                    if (accState.Account != null)
                    {
                        var account = Session.Get<DBAccount>(accState.Account.Id);
                        if (account != null)
                        {
                            account.Balance = accState.Balance;
                            account.Lastupdate = DateTime.UtcNow;
                            Session.Update(account);
                        }
                    }

                    Transaction.Commit();
                }
            }
        }
        catch (Exception e)
        {
            log.Error("Error: SaveInsertWaletState: " + e);
        }
    }

    public Person LoginPerson(string mail, string password)
    {
        var result = persons.FindUser(mail, password);
        return result;
    }

    public IList<T> ExecuteNativeQuery<T>(ISession session, string queryString, string entityParamName,
        Tuple<string, object, IType>[] parameters = null)
    {
        var query = session.CreateSQLQuery(queryString).AddEntity(entityParamName, typeof(T));
        if (parameters != null)
            for (var i = 0; i < parameters.Length; i++)
                query.SetParameter(parameters[i].Item1, parameters[i].Item2, parameters[i].Item3);

        return query.List<T>();
    }

    public object GetObjects(EntitiesEnum type, bool showRetired)
    {
        var t = MainService.thisGlobal.EnumToType(type);
        if (t == null)
            return "";
        var result = new List<object>();
        using (var Session = ConnectionHelper.CreateNewSession())
        {
            Session.Query<object>(t.Item1.FullName)
                .ToList()
                .ForEach(item =>
                {
                    var dtObj = mapper.Map(item, t.Item1, t.Item2);
                    if (dtObj is Idable obj)
                    {
                        if (showRetired)
                            result.Add(dtObj); 
                        else
                        {
                            if (!obj.Retired)
                                result.Add(dtObj);
                        }
                    }
                });
            return result;
        }
    }
    
    public object GetChildObjects(EntitiesEnum parentType, EntitiesEnum childType, int parentKey, bool showRetired)
    {
        var parentTuple = MainService.thisGlobal.EnumToType(parentType);
        if (parentTuple == null)
            return "";
        var childTuple = MainService.thisGlobal.EnumToType(childType);
        if (childTuple == null)
            return "";

        var result = new List<object>();
        using (var Session = ConnectionHelper.CreateNewSession())
        {
            if (parentType == EntitiesEnum.MetaSymbol && childType == EntitiesEnum.Symbol)
            {
                var list = Session.QueryOver<DBSymbol>()
                    .JoinQueryOver(master => master.Metasymbol)
                    .Where(parent => parent.Id == parentKey)
                    .List();
                if (list.Any())
                {
                    foreach (var item in list)
                    {
                        object dtObj = null;
                        try
                        {
                            if (showRetired == false && item.Retired == 1)
                                continue;
                            dtObj = mapper.Map(item, childTuple.Item1, childTuple.Item2);
                            result.Add(dtObj); 
                        }
                        catch (Exception e)
                        {
                            log.Log($"Mapping DBSymbol {item.Id} {item.Name} error: {e.ToString()}");
                            continue;
                        }
                    }

                    return result;
                }
            }

            if (parentType == EntitiesEnum.MetaSymbol && childType == EntitiesEnum.Adviser)
            {
                var list = Session.QueryOver<DBAdviser>()
                    .JoinQueryOver(master => master.Symbol)
                    .JoinQueryOver(master => master.Metasymbol)
                    .Where(parent => parent.Id == parentKey)
                    .List();
                if (list.Any())
                {
                    foreach (var item in list)
                    {
                        object dtObj = null;
                        try
                        {
                            if (showRetired == false && item.Retired == true)
                                continue;
                            dtObj = mapper.Map(item, childTuple.Item1, childTuple.Item2);
                            result.Add(dtObj); 
                        }
                        catch (Exception e)
                        {
                            log.Log($"Mapping DBAdviser {item.Id} {item.Name} error: {e.ToString()}");
                            continue;
                        }                        
                    }

                    return result;
                }
            }

            if (parentType == EntitiesEnum.Wallet && childType == EntitiesEnum.Account)
            {
                var list = Session.QueryOver<DBAccount>()
                    .JoinQueryOver(master => master.Wallet)
                    .Where(parent => parent.Id == parentKey)
                    .List();
                if (list.Any())
                {
                    foreach (var item in list)
                    {
                        object dtObj = null;
                        try
                        {
                            if (showRetired == false && item.Retired == true)
                                continue;
                            dtObj = mapper.Map(item, childTuple.Item1, childTuple.Item2);
                            result.Add(dtObj); 
                        }
                        catch (Exception e)
                        {
                            log.Log($"Mapping DBAccount {item.Id} {item.Description} error: {e.ToString()}");
                            log.Log(e.ToString());
                            continue;
                        }
                    }

                    return result;
                }
            }

            if (parentType == EntitiesEnum.Wallet && childType == EntitiesEnum.Terminal)
            {
                var list = Session.QueryOver<DBTerminal>()
                    .JoinQueryOver(master => master.Account)
                    .JoinQueryOver(master => master.Wallet)
                    .Where(parent => parent.Id == parentKey)
                    .List();
                if (list.Any())
                {
                    foreach (var item in list)
                    {
                        object dtObj = null;
                        try
                        {
                            if (showRetired == false && item.Retired == true)
                                continue;
                            dtObj = mapper.Map(item, childTuple.Item1, childTuple.Item2);
                            result.Add(dtObj); 
                        }
                        catch (Exception e)
                        {
                            log.Log($"Mapping DBTerminal {item.Id} {item.Accountnumber} error: {e.ToString()}");
                            continue;
                        }
                    }
                    return result;
                }
            }
            return result;
        }
    }

    public object GetObject(EntitiesEnum type, int id)
    {
        var t = MainService.thisGlobal.EnumToType(type);
        if (t == null)
            return "";
        var result = new List<object>();
        using (var Session = ConnectionHelper.CreateNewSession())
        {
            var dbObj = Session.Get(t.Item1, id);
            if (dbObj == null)
                return "";
            var obj = mapper.Map(dbObj, t.Item1, t.Item2);
            return obj;
        }
    }

    public int InsertObject(EntitiesEnum type, string values)
    {
        var tuple = MainService.thisGlobal.EnumToType(type);
        if (tuple == null)
            return -1;
        using (var Session = ConnectionHelper.CreateNewSession())
        {
            var obj = JsonConvert.DeserializeObject(values, tuple.Item2);
            if (obj == null)
                return -1;
            using (var Transaction = Session.BeginTransaction())
            {
                var dbNew = mapper.Map(obj, tuple.Item2, tuple.Item1);
                var id = Session.Save(dbNew);
                Transaction.Commit();
                return (int) id;
            }
        }
    }

    public int UpdateObject(EntitiesEnum type, int id, string values)
    {
        using (var Session = ConnectionHelper.CreateNewSession())
        {
            var tuple = MainService.thisGlobal.EnumToType(type);
            if (tuple == null)
                return -1;
            var obj = JsonConvert.DeserializeObject(values, tuple.Item2);
            if (obj == null)
                return -1;
            var dbObj = Session.Get(tuple.Item1, id);
            if (dbObj == null)
                return -1;
            using (var Transaction = Session.BeginTransaction())
            {
                var dbNew = mapper.Map(obj, dbObj, tuple.Item2, tuple.Item1);
                Session.Update(dbNew);
                Transaction.Commit();
                return 1;
            }
        }
    }

    public int DeleteObject(EntitiesEnum type, int id)
    {
        var tuple = MainService.thisGlobal.EnumToType(type);
        if (tuple == null)
            return -1;
        using (var Session = ConnectionHelper.CreateNewSession())
        {
            using (var Transaction = Session.BeginTransaction())
            {
                Session.Delete(Session.Load(tuple.Item1, id));
                Transaction.Commit();
            }
        }

        return 1;
    }

    #endregion
}
