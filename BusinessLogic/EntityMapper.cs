using System;
using System.Collections.Generic;
using BusinessLogic.Repo;
using BusinessLogic.Repo.Domain;
using BusinessObjects.BusinessObjects;

namespace BusinessLogic;

/// <summary>
///     Hand-written replacement for the former AutoMapper 12 MappingProfile.
///     Every map reproduces what AutoMapper actually did (explicit ForMember/MapFrom/Ignore,
///     same-name convention mapping, flattening, ReverseMap unflattening and the implicit
///     type conversions). Destination members that AutoMapper did not map are left untouched,
///     so mapping into an existing destination behaves like IMapper.Map(src, dest).
///     None of the maps recurse into nested object maps, so there are no reference cycles.
/// </summary>
public static class EntityMapper
{
    private static readonly Dictionary<(Type, Type), Func<object, object, object>> maps = new();

    static EntityMapper()
    {
        Register<DBAccount, Account>(Map);
        Register<Account, DBAccount>(Map);
        Register<DBAdviser, Adviser>(Map);
        Register<Adviser, DBAdviser>(Map);
        Register<DBDeals, DealInfo>(Map);
        Register<DealInfo, DBDeals>(Map);
        Register<DBAccountstate, AccountState>(Map);
        Register<AccountState, DBAccountstate>(Map);
        Register<DBRates, Rates>(Map);
        Register<Rates, DBRates>(Map);
        Register<DBWallet, Wallet>(Map);
        Register<Wallet, DBWallet>(Map);
        Register<DBTerminal, Terminal>(Map); // no reverse map existed
        Register<DBJobs, ScheduledJobView>(Map); // no reverse map existed
        Register<DBPerson, Person>(Map);
        Register<Person, DBPerson>(Map);
        Register<DBSymbol, Symbol>(Map);
        Register<Symbol, DBSymbol>(Map);
        Register<DBSettings, Settings>(Map);
        Register<Settings, DBSettings>(Map);
        Register<DBMetasymbol, MetaSymbol>(Map);
        Register<MetaSymbol, DBMetasymbol>(Map);
    }

    /// <summary>
    ///     Test seam for the int -> entity lookups (formerly AutoMapper ConvertUsing&lt;EntityConverter&lt;T&gt;&gt;).
    ///     When null, the entity is loaded through <see cref="EntityConverter{T}" /> (BaseRepository.GetById).
    /// </summary>
    public static Func<Type, int, object> EntityLookup { get; set; }

    private static void Register<TSrc, TDst>(Func<TSrc, TDst, TDst> map) where TDst : new()
    {
        maps[(typeof(TSrc), typeof(TDst))] = (src, dest) => map((TSrc)src, dest == null ? new TDst() : (TDst)dest);
    }

    public static bool CanMap(Type sourceType, Type destinationType)
    {
        return maps.ContainsKey((sourceType, destinationType));
    }

    /// <summary>Equivalent of IMapper.Map(source, sourceType, destinationType): returns a new instance.</summary>
    public static object Map(object source, Type sourceType, Type destinationType)
    {
        return Map(source, null, sourceType, destinationType);
    }

    /// <summary>
    ///     Equivalent of IMapper.Map(source, destination, sourceType, destinationType): maps into
    ///     <paramref name="destination" /> in place (a new instance is created when it is null) and returns it.
    /// </summary>
    public static object Map(object source, object destination, Type sourceType, Type destinationType)
    {
        if (!maps.TryGetValue((sourceType, destinationType), out var map))
            throw new NotSupportedException(
                $"Missing type map configuration or unsupported mapping. {sourceType?.Name} -> {destinationType?.Name}");
        if (source == null)
            return destination;
        return map(source, destination);
    }

    public static TDst Map<TSrc, TDst>(TSrc source, TDst destination = default)
    {
        return (TDst)Map(source, destination, typeof(TSrc), typeof(TDst));
    }

    private static T LoadEntity<T>(int id)
    {
        var lookup = EntityLookup;
        if (lookup != null)
            return (T)lookup(typeof(T), id);
        return new EntityConverter<T>().Convert(id);
    }

    #region Account

    public static Account Map(DBAccount s, Account d)
    {
        d.Description = s.Description;
        d.Balance = s.Balance;
        d.Equity = s.Equity;
        d.CurrencyId = s.Currency?.Id ?? 0;
        d.CurrencyStr = s.Currency?.Name;
        d.WalletId = s.Wallet?.Id ?? 0;
        d.TerminalId = s.Terminal?.Id ?? 0; // convention flattening Terminal.Id
        d.PersonId = s.Person?.Id ?? 0;
        d.Number = s.Number;
        d.Lastupdate = s.Lastupdate;
        d.Retired = s.Retired;
        d.Typ = (AccountType)s.Typ;
        d.Id = s.Id;
        // DailyProfit, DailyProfitPercent, DailyMaxGain, StopTrading, StopReason: not mapped
        return d;
    }

    public static DBAccount Map(Account s, DBAccount d)
    {
        d.Id = s.Id;
        d.Number = Convert.ToInt32(s.Number); // long -> int, throws OverflowException like AutoMapper
        d.Description = s.Description;
        d.Balance = s.Balance;
        d.Equity = s.Equity;
        d.Lastupdate = s.Lastupdate;
        d.Retired = s.Retired;
        d.Typ = (int)s.Typ;
        // ReverseMap unflattening of the MapFrom paths: creates the related object when null,
        // otherwise overwrites the properties of the existing related object.
        d.Currency ??= new DBCurrency();
        d.Currency.Name = s.CurrencyStr;
        d.Currency.Id = s.CurrencyId;
        d.Wallet ??= new DBWallet();
        d.Wallet.Id = s.WalletId;
        d.Person ??= new DBPerson();
        d.Person.Id = s.PersonId;
        // Terminal: ignored
        return d;
    }

    #endregion

    #region Adviser

    public static Adviser Map(DBAdviser s, Adviser d)
    {
        d.Name = s.Name;
        d.AccountNumber = s.Terminal?.Accountnumber ?? 0;
        d.Broker = s.Terminal?.Broker;
        d.FullPath = s.Terminal?.Fullpath;
        d.CodeBase = s.Terminal?.Codebase;
        d.TerminalId = s.Terminal?.Id ?? 0;
        d.SymbolId = s.Symbol?.Id ?? 0;
        d.Symbol = s.Symbol?.Name;
        d.MetaSymbol = s.Symbol?.Metasymbol?.Name;
        d.MetaSymbolId = s.Symbol?.Metasymbol?.Id ?? 0;
        d.Timeframe = s.Timeframe;
        d.Running = s.Running;
        d.Retired = s.Retired;
        d.IsMaster = s.IsMaster;
        d.Id = s.Id;
        return d;
    }

    public static DBAdviser Map(Adviser s, DBAdviser d)
    {
        d.Id = s.Id;
        d.Name = s.Name;
        d.Timeframe = s.Timeframe;
        d.IsMaster = s.IsMaster;
        d.Running = s.Running;
        d.Retired = s.Retired;
        // Terminal, Symbol: ignored (so their reverse paths are ignored too)
        return d;
    }

    #endregion

    #region Deals

    public static DealInfo Map(DBDeals s, DealInfo d)
    {
        d.Ticket = s.Orderid ?? 0;
        d.Symbol = s.Symbol?.Name;
        d.Lots = Convert.ToDouble(s.Volume);
        d.OpenTime = s.Opentime.ToString(xtradeConstants.MTDATETIMEFORMAT);
        d.CloseTime = s.Closetime?.ToString(xtradeConstants.MTDATETIMEFORMAT);
        d.Profit = Convert.ToDouble(s.Profit);
        d.Swap = Convert.ToDouble(s.Swap);
        d.Commission = Convert.ToDouble(s.Commission);
        d.Comment = s.Comment;
        d.OrderId = s.Orderid ?? 0; // convention (case-insensitive) Orderid -> OrderId
        // Type, OpenPrice, ClosePrice, Account, AccountName: not mapped
        return d;
    }

    public static DBDeals Map(DealInfo s, DBDeals d)
    {
        d.Orderid = Convert.ToInt32(s.Ticket); // reverse of Ticket <- Orderid (wins over OrderId)
        d.Volume = Convert.ToDecimal(s.Lots);
        // convention string -> DateTime (current culture); null -> DateTime.MinValue / null
        d.Opentime = Convert.ToDateTime(s.OpenTime);
        d.Closetime = s.CloseTime == null ? null : Convert.ToDateTime(s.CloseTime);
        d.Comment = s.Comment;
        d.Commission = Convert.ToDecimal(s.Commission);
        d.Swap = Convert.ToDecimal(s.Swap);
        d.Profit = Convert.ToDecimal(s.Profit);
        // Id, Terminal, Symbol (ignored), Dealid, Typ, Price: not mapped
        return d;
    }

    #endregion

    #region AccountState

    public static AccountState Map(DBAccountstate s, AccountState d)
    {
        // AutoMapper had MapFrom(ol => ol.Account) (DBAccount -> int) without a type map, so it threw for
        // any non-null Account. Map the account id, which is what that MapFrom intended.
        d.AccountId = s.Account?.Id ?? 0;
        d.Balance = s.Balance;
        d.Comment = s.Comment;
        d.Date = s.Date;
        d.Id = s.Id;
        // Retired: ignored
        return d;
    }

    public static DBAccountstate Map(AccountState s, DBAccountstate d)
    {
        d.Account = LoadEntity<DBAccount>(s.AccountId); // int -> DBAccount via EntityConverter
        d.Balance = s.Balance;
        d.Comment = s.Comment;
        d.Date = s.Date;
        d.Id = s.Id;
        return d;
    }

    #endregion

    #region Rates

    public static Rates Map(DBRates s, Rates d)
    {
        d.MetaSymbol = s.Metasymbol?.Name;
        d.C1 = s.Metasymbol?.C1;
        d.C2 = s.Metasymbol?.C2;
        d.Symbol = s.Symbol;
        d.Ratebid = s.Ratebid;
        d.Rateask = s.Rateask;
        d.Lastupdate = s.Lastupdate ?? default;
        d.Retired = s.Retired;
        d.Id = s.Id;
        return d;
    }

    public static DBRates Map(Rates s, DBRates d)
    {
        d.Id = s.Id;
        d.Symbol = s.Symbol;
        d.Ratebid = s.Ratebid;
        d.Rateask = s.Rateask;
        d.Lastupdate = s.Lastupdate;
        d.Retired = s.Retired;
        // Metasymbol: ignored
        return d;
    }

    #endregion

    #region Wallet

    public static Wallet Map(DBWallet s, Wallet d)
    {
        d.PersonId = s.Person?.Id ?? 0; // convention flattening
        d.SiteId = s.Site?.Id ?? 0; // convention flattening
        d.Name = s.Name;
        d.Shortname = s.Shortname;
        d.Retired = s.Retired;
        d.Id = s.Id;
        // Balance, Date, Accounts: not mapped
        return d;
    }

    public static DBWallet Map(Wallet s, DBWallet d)
    {
        d.Id = s.Id;
        d.Name = s.Name;
        d.Shortname = s.Shortname;
        d.Retired = s.Retired;
        // Person, Site: ignored; Link: not mapped
        return d;
    }

    #endregion

    #region Terminal

    public static Terminal Map(DBTerminal s, Terminal d)
    {
        d.AccountNumber = s.Accountnumber ?? 0;
        d.Broker = s.Broker;
        d.FullPath = s.Fullpath;
        d.CodeBase = s.Codebase;
        d.Retired = s.Retired;
        d.Stopped = s.Stopped;
        d.Currency = s.Account?.Currency?.Name;
        d.Id = s.Id;
        return d;
    }

    #endregion

    #region Jobs

    public static ScheduledJobView Map(DBJobs s, ScheduledJobView d)
    {
        d.PrevDate = s.Prevdate;
        d.NextDate = s.Nextdate;
        d.Group = s.Grp;
        d.Name = s.Name;
        d.Schedule = s.Cron;
        d.Log = s.Statmessage;
        d.Id = s.Id;
        // IsRunning: ignored; Retired: no source member
        return d;
    }

    #endregion

    #region Person

    public static Person Map(DBPerson s, Person d)
    {
        d.Created = s.Created;
        d.Credential = s.Credential;
        d.Regip = s.Regip;
        d.Mail = s.Mail;
        d.Privilege = s.Privilege;
        d.Uuid = s.Uuid;
        d.Activated = s.Activated;
        d.Retired = s.Retired;
        d.Id = s.Id;
        // CountryId, Languageid: ignored
        return d;
    }

    public static DBPerson Map(Person s, DBPerson d)
    {
        d.Id = s.Id;
        d.Created = s.Created;
        d.Languageid = s.Languageid; // the forward Ignore does not apply to the reverse map
        d.Credential = s.Credential;
        d.Regip = s.Regip;
        d.Mail = s.Mail;
        d.Privilege = s.Privilege;
        d.Uuid = s.Uuid;
        d.Activated = s.Activated;
        d.Retired = s.Retired;
        // Country: ignored
        return d;
    }

    #endregion

    #region Symbol

    public static Symbol Map(DBSymbol s, Symbol d)
    {
        d.MetasymbolId = s.Metasymbol?.Id ?? 0;
        d.Name = s.Name;
        d.Description = s.Description;
        d.Expiration = s.Expiration;
        d.Retired = s.Retired.HasValue && Convert.ToBoolean(s.Retired.Value); // int? -> bool, non-zero is true
        d.Id = s.Id;
        return d;
    }

    public static DBSymbol Map(Symbol s, DBSymbol d)
    {
        d.Metasymbol = LoadEntity<DBMetasymbol>(s.MetasymbolId); // int -> DBMetasymbol via EntityConverter
        d.Name = s.Name;
        d.Description = s.Description;
        d.Expiration = s.Expiration;
        d.Retired = Convert.ToInt32(s.Retired);
        d.Id = s.Id;
        return d;
    }

    #endregion

    #region Settings

    public static Settings Map(DBSettings s, Settings d)
    {
        d.Id = s.Id;
        d.Propertyname = s.Propertyname;
        d.Value = s.Value;
        d.Description = s.Description;
        // Retired: no source member
        return d;
    }

    public static DBSettings Map(Settings s, DBSettings d)
    {
        d.Id = s.Id;
        d.Propertyname = s.Propertyname;
        d.Value = s.Value;
        d.Description = s.Description;
        return d;
    }

    #endregion

    #region MetaSymbol

    public static MetaSymbol Map(DBMetasymbol s, MetaSymbol d)
    {
        d.Id = s.Id;
        d.Name = s.Name;
        d.Description = s.Description;
        d.C1 = s.C1;
        d.C2 = s.C2;
        d.Typ = s.Typ;
        d.Retired = s.Retired;
        return d;
    }

    public static DBMetasymbol Map(MetaSymbol s, DBMetasymbol d)
    {
        d.Id = s.Id;
        d.Name = s.Name;
        d.Description = s.Description;
        d.C1 = s.C1;
        d.C2 = s.C2;
        d.Typ = s.Typ;
        d.Retired = s.Retired;
        return d;
    }

    #endregion
}
