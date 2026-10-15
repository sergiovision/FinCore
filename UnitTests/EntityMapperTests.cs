using BusinessLogic;
using BusinessLogic.Repo.Domain;
using BusinessObjects.BusinessObjects;

// EntityMapper.EntityLookup is a static seam, so tests must not run in parallel.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace UnitTests;

/// <summary>
///     Expected values were verified against the original AutoMapper 12.0.1 profile
///     (same maps, run side by side before AutoMapper was removed).
/// </summary>
public class EntityMapperTests : IDisposable
{
    private static readonly DateTime D1 = new(2021, 3, 4, 5, 6, 7);
    private static readonly DateTime D2 = new(2022, 8, 9, 10, 11, 12);
    private readonly List<(Type Type, int Id)> lookups = new();

    public EntityMapperTests()
    {
        EntityMapper.EntityLookup = (type, id) =>
        {
            lookups.Add((type, id));
            var entity = Activator.CreateInstance(type);
            type.GetProperty("Id")!.SetValue(entity, id);
            return entity;
        };
    }

    public void Dispose()
    {
        EntityMapper.EntityLookup = null;
    }

    private static TDst Map<TSrc, TDst>(TSrc src)
    {
        return (TDst)EntityMapper.Map(src, typeof(TSrc), typeof(TDst));
    }

    private static TDst MapInto<TSrc, TDst>(TSrc src, TDst dest) where TDst : class
    {
        var result = EntityMapper.Map(src, dest, typeof(TSrc), typeof(TDst));
        Assert.Same(dest, result);
        return dest;
    }

    private static DBPerson NewDbPerson()
    {
        return new DBPerson
        {
            Id = 12, Country = new DBCountry { Id = 31, Code = "RU", Description = "Russia" }, Created = D1,
            Languageid = 5, Credential = "cred", Regip = "1.2.3.4", Mail = "a@b", Privilege = "adm", Uuid = "uuid",
            Activated = 3, Retired = true
        };
    }

    private static DBMetasymbol NewDbMeta()
    {
        return new DBMetasymbol
            { Id = 14, Name = "EURUSD", Description = "md", C1 = "EUR", C2 = "USD", Typ = 2, Retired = true };
    }

    private static DBSymbol NewDbSymbol()
    {
        return new DBSymbol
            { Id = 15, Metasymbol = NewDbMeta(), Name = "EURUSDx", Description = "sdesc", Retired = 1, Expiration = D2 };
    }

    private static DBAccount NewDbAccount()
    {
        var a = new DBAccount
        {
            Id = 16, Currency = new DBCurrency { Id = 11, Name = "USD", Enabled = 1 },
            Wallet = new DBWallet { Id = 13, Name = "wal" }, Person = NewDbPerson(), Number = 123456,
            Description = "acc", Balance = 100.5m, Equity = 99.5m, Lastupdate = D1, Retired = true, Typ = 1
        };
        a.Terminal = new DBTerminal
        {
            Id = 17, Account = a, Accountnumber = 777, Broker = "br", Fullpath = "fp", Codebase = "cb", Retired = true,
            Stopped = true
        };
        return a;
    }

    // ---------------- Account ----------------

    [Fact]
    public void DBAccount_To_Account()
    {
        var d = Map<DBAccount, Account>(NewDbAccount());
        Assert.Equal("acc", d.Description);
        Assert.Equal(100.5m, d.Balance);
        Assert.Equal(99.5m, d.Equity);
        Assert.Equal(11, d.CurrencyId);
        Assert.Equal("USD", d.CurrencyStr);
        Assert.Equal(13, d.WalletId);
        Assert.Equal(17, d.TerminalId); // convention flattening
        Assert.Equal(12, d.PersonId);
        Assert.Equal(123456L, d.Number);
        Assert.Equal(D1, d.Lastupdate);
        Assert.True(d.Retired);
        Assert.Equal(AccountType.Investment, d.Typ);
        Assert.Equal(16, d.Id);
        // not mapped
        Assert.Equal(0m, d.DailyProfit);
        Assert.Equal(0m, d.DailyProfitPercent);
        Assert.Equal(0m, d.DailyMaxGain);
        Assert.False(d.StopTrading);
        Assert.Null(d.StopReason);
    }

    [Fact]
    public void DBAccount_To_Account_NullNavigations()
    {
        var d = Map<DBAccount, Account>(new DBAccount { Id = 1, Typ = 5 });
        Assert.Equal(0, d.CurrencyId);
        Assert.Null(d.CurrencyStr);
        Assert.Equal(0, d.WalletId);
        Assert.Equal(0, d.TerminalId);
        Assert.Equal(0, d.PersonId);
        Assert.Equal((AccountType)5, d.Typ);
        Assert.Equal(1, d.Id);
    }

    private static Account NewAccountDto()
    {
        return new Account
        {
            Description = "d2", Balance = 1.25m, Equity = 2.5m, CurrencyId = 21, CurrencyStr = "EUR", WalletId = 22,
            TerminalId = 23, PersonId = 24, Number = 98765, Lastupdate = D2, Retired = true,
            Typ = AccountType.Investment, DailyProfit = 3, DailyProfitPercent = 4, DailyMaxGain = 5,
            StopTrading = true, StopReason = "sr", Id = 25
        };
    }

    [Fact]
    public void Account_To_DBAccount_New()
    {
        var d = Map<Account, DBAccount>(NewAccountDto());
        Assert.Equal(25, d.Id);
        Assert.Equal(98765, d.Number);
        Assert.Equal("d2", d.Description);
        Assert.Equal(1.25m, d.Balance);
        Assert.Equal(2.5m, d.Equity);
        Assert.Equal(D2, d.Lastupdate);
        Assert.True(d.Retired);
        Assert.Equal(1, d.Typ);
        // unflattened from the reversed MapFrom paths
        Assert.Equal(21, d.Currency.Id);
        Assert.Equal("EUR", d.Currency.Name);
        Assert.Null(d.Currency.Enabled);
        Assert.Equal(22, d.Wallet.Id);
        Assert.Null(d.Wallet.Name);
        Assert.Equal(24, d.Person.Id);
        Assert.Null(d.Person.Mail);
        Assert.Null(d.Terminal); // ignored
        Assert.Empty(lookups);
    }

    [Fact]
    public void Account_To_DBAccount_UpdateInPlace_MutatesExistingRelatedObjects()
    {
        var existing = NewDbAccount();
        var currency = existing.Currency;
        var wallet = existing.Wallet;
        var person = existing.Person;
        var terminal = existing.Terminal;
        MapInto(NewAccountDto(), existing);
        Assert.Same(currency, existing.Currency);
        Assert.Same(wallet, existing.Wallet);
        Assert.Same(person, existing.Person);
        Assert.Same(terminal, existing.Terminal);
        Assert.Equal(21, currency.Id);
        Assert.Equal("EUR", currency.Name);
        Assert.Equal(1, currency.Enabled); // untouched
        Assert.Equal(22, wallet.Id);
        Assert.Equal("wal", wallet.Name); // untouched
        Assert.Equal(24, person.Id);
        Assert.Equal("a@b", person.Mail); // untouched
        Assert.Equal(25, existing.Id);
        Assert.Equal("d2", existing.Description);
    }

    [Fact]
    public void Account_To_DBAccount_NumberOverflowThrows()
    {
        Assert.Throws<OverflowException>(() => Map<Account, DBAccount>(new Account { Number = long.MaxValue }));
    }

    // ---------------- Adviser ----------------

    [Fact]
    public void DBAdviser_To_Adviser()
    {
        var src = new DBAdviser
        {
            Id = 41, Terminal = NewDbAccount().Terminal, Symbol = NewDbSymbol(), Name = "adv", Timeframe = "H1",
            IsMaster = true, Running = true, Retired = true
        };
        var d = Map<DBAdviser, Adviser>(src);
        Assert.Equal("adv", d.Name);
        Assert.Equal(777L, d.AccountNumber);
        Assert.Equal("br", d.Broker);
        Assert.Equal("fp", d.FullPath);
        Assert.Equal("cb", d.CodeBase);
        Assert.Equal(17, d.TerminalId);
        Assert.Equal(15, d.SymbolId);
        Assert.Equal("EURUSDx", d.Symbol);
        Assert.Equal("EURUSD", d.MetaSymbol);
        Assert.Equal(14, d.MetaSymbolId);
        Assert.Equal("H1", d.Timeframe);
        Assert.True(d.Running);
        Assert.True(d.Retired);
        Assert.True(d.IsMaster);
        Assert.Equal(41, d.Id);
    }

    [Fact]
    public void DBAdviser_To_Adviser_NullNavigations()
    {
        var d = Map<DBAdviser, Adviser>(new DBAdviser
            { Id = 2, Symbol = new DBSymbol { Id = 3, Name = "s" }, Terminal = new DBTerminal { Id = 4 } });
        Assert.Equal(0L, d.AccountNumber);
        Assert.Equal(4, d.TerminalId);
        Assert.Equal(3, d.SymbolId);
        Assert.Equal("s", d.Symbol);
        Assert.Null(d.MetaSymbol);
        Assert.Equal(0, d.MetaSymbolId);

        var empty = Map<DBAdviser, Adviser>(new DBAdviser { Id = 5 });
        Assert.Equal(0, empty.TerminalId);
        Assert.Equal(0, empty.SymbolId);
        Assert.Null(empty.Broker);
    }

    [Fact]
    public void Adviser_To_DBAdviser()
    {
        var src = new Adviser
        {
            Name = "n2", AccountNumber = 555, Broker = "b2", FullPath = "f2", CodeBase = "c2", TerminalId = 61,
            SymbolId = 62, Symbol = "S2", MetaSymbol = "M2", MetaSymbolId = 63, Timeframe = "M5", Running = true,
            Retired = true, IsMaster = true, Id = 64
        };
        var d = Map<Adviser, DBAdviser>(src);
        Assert.Equal(64, d.Id);
        Assert.Equal("n2", d.Name);
        Assert.Equal("M5", d.Timeframe);
        Assert.True(d.IsMaster);
        Assert.True(d.Running);
        Assert.True(d.Retired);
        Assert.Null(d.Terminal);
        Assert.Null(d.Symbol);

        var terminal = new DBTerminal { Id = 17, Broker = "br" };
        var symbol = NewDbSymbol();
        var existing = MapInto(src, new DBAdviser { Id = 41, Terminal = terminal, Symbol = symbol });
        Assert.Same(terminal, existing.Terminal);
        Assert.Same(symbol, existing.Symbol);
        Assert.Equal(17, terminal.Id);
        Assert.Equal(15, symbol.Id);
        Assert.Equal(64, existing.Id);
        Assert.Empty(lookups);
    }

    // ---------------- Deals ----------------

    [Fact]
    public void DBDeals_To_DealInfo()
    {
        var src = new DBDeals
        {
            Id = 71, Terminal = NewDbAccount().Terminal, Symbol = NewDbSymbol(), Orderid = 72, Dealid = 73,
            Opentime = D1, Typ = 1, Volume = 0.5m, Price = 1.2345m, Closetime = D2, Comment = "cm",
            Commission = -1.5m, Swap = -0.25m, Profit = 12.75m
        };
        var d = Map<DBDeals, DealInfo>(src);
        Assert.Equal(72L, d.Ticket);
        Assert.Equal("EURUSDx", d.Symbol);
        Assert.Equal(0.5, d.Lots);
        Assert.Equal(D1.ToString(xtradeConstants.MTDATETIMEFORMAT), d.OpenTime);
        Assert.Equal(D2.ToString(xtradeConstants.MTDATETIMEFORMAT), d.CloseTime);
        Assert.Equal(12.75, d.Profit);
        Assert.Equal(-0.25, d.Swap);
        Assert.Equal(-1.5, d.Commission);
        Assert.Equal("cm", d.Comment);
        Assert.Equal(72L, d.OrderId); // convention Orderid -> OrderId
        // not mapped (Typ != Type, Price != OpenPrice)
        Assert.Equal(0, d.Type);
        Assert.Equal(0.0, d.OpenPrice);
        Assert.Equal(0.0, d.ClosePrice);
        Assert.Equal(0L, d.Account);
        Assert.Null(d.AccountName);
    }

    [Fact]
    public void DBDeals_To_DealInfo_Nulls()
    {
        var d = Map<DBDeals, DealInfo>(new DBDeals { Id = 3 });
        Assert.Equal(0L, d.Ticket);
        Assert.Equal(0L, d.OrderId);
        Assert.Null(d.Symbol);
        Assert.Null(d.CloseTime);
        Assert.Equal(DateTime.MinValue.ToString(xtradeConstants.MTDATETIMEFORMAT), d.OpenTime);
    }

    [Fact]
    public void DealInfo_To_DBDeals()
    {
        var open = new DateTime(2020, 1, 2, 3, 4, 0);
        var close = new DateTime(2020, 2, 3, 4, 5, 0);
        var src = new DealInfo
        {
            Ticket = 81, Type = 1, Symbol = "S", Lots = 0.25, OpenPrice = 1.1, ClosePrice = 1.2,
            OpenTime = open.ToString("s"), CloseTime = close.ToString("s"), Profit = 9.5, Swap = 0.5,
            Commission = 1.5, Account = 82, AccountName = "an", Comment = "c", OrderId = 83
        };
        var d = Map<DealInfo, DBDeals>(src);
        Assert.Equal(81, d.Orderid); // reversed Ticket wins over OrderId
        Assert.Equal(open, d.Opentime);
        Assert.Equal(close, d.Closetime);
        Assert.Equal(0.25m, d.Volume);
        Assert.Equal(9.5m, d.Profit);
        Assert.Equal(0.5m, d.Swap);
        Assert.Equal(1.5m, d.Commission);
        Assert.Equal("c", d.Comment);
        Assert.Equal(0, d.Id);
        Assert.Equal(0, d.Typ);
        Assert.Equal(0m, d.Price);
        Assert.Null(d.Dealid);
        Assert.Null(d.Symbol);
        Assert.Null(d.Terminal);

        var symbol = NewDbSymbol();
        var existing = MapInto(src, new DBDeals { Id = 71, Symbol = symbol, Dealid = 73, Typ = 5, Price = 2m });
        Assert.Equal(71, existing.Id);
        Assert.Same(symbol, existing.Symbol);
        Assert.Equal(73, existing.Dealid);
        Assert.Equal(5, existing.Typ);
        Assert.Equal(2m, existing.Price);
        Assert.Equal(81, existing.Orderid);
    }

    [Fact]
    public void DealInfo_To_DBDeals_NullAndInvalidTimes()
    {
        var d = MapInto(new DealInfo(), new DBDeals { Opentime = D1, Closetime = D2, Orderid = 5 });
        Assert.Equal(DateTime.MinValue, d.Opentime);
        Assert.Null(d.Closetime);
        Assert.Equal(0, d.Orderid);
        Assert.Throws<FormatException>(() => Map<DealInfo, DBDeals>(new DealInfo { OpenTime = "" }));
        Assert.Throws<FormatException>(() =>
            Map<DealInfo, DBDeals>(new DealInfo { OpenTime = "2020-01-01", CloseTime = "garbage" }));
        Assert.Throws<OverflowException>(() =>
            Map<DealInfo, DBDeals>(new DealInfo { OpenTime = "2020-01-01", Lots = double.NaN }));
    }

    // ---------------- AccountState ----------------

    [Fact]
    public void DBAccountstate_To_AccountState_WithoutAccount()
    {
        var d = MapInto(new DBAccountstate { Id = 91, Date = D1, Balance = 55.5m, Comment = "cmt" },
            new AccountState { AccountId = 9, Retired = true });
        Assert.Equal(0, d.AccountId);
        Assert.Equal(D1, d.Date);
        Assert.Equal(55.5m, d.Balance);
        Assert.Equal("cmt", d.Comment);
        Assert.Equal(91, d.Id);
        Assert.True(d.Retired); // ignored
    }

    [Fact]
    public void DBAccountstate_To_AccountState_WithAccount_MapsAccountId()
    {
        var account = NewDbAccount();
        var d = Map<DBAccountstate, AccountState>(new DBAccountstate { Id = 91, Account = account });
        Assert.Equal(account.Id, d.AccountId);
        Assert.Equal(91, d.Id);
    }

    [Fact]
    public void AccountState_To_DBAccountstate_LoadsAccountById()
    {
        var src = new AccountState { AccountId = 92, Date = D2, Balance = 66.5m, Comment = "c2", Id = 93, Retired = true };
        var d = Map<AccountState, DBAccountstate>(src);
        Assert.Equal(92, d.Account.Id);
        Assert.Equal(D2, d.Date);
        Assert.Equal(66.5m, d.Balance);
        Assert.Equal("c2", d.Comment);
        Assert.Equal(93, d.Id);
        Assert.Equal(new[] { (typeof(DBAccount), 92) }, lookups);

        var old = NewDbAccount();
        var existing = MapInto(src, new DBAccountstate { Id = 91, Account = old });
        Assert.NotSame(old, existing.Account);
        Assert.Equal(92, existing.Account.Id);
    }

    // ---------------- Rates ----------------

    [Fact]
    public void DBRates_To_Rates()
    {
        var d = Map<DBRates, Rates>(new DBRates
        {
            Id = 101, Metasymbol = NewDbMeta(), Symbol = "EURUSD.x", Ratebid = 1.1m, Rateask = 1.2m, Lastupdate = D1,
            Retired = true
        });
        Assert.Equal("EURUSD", d.MetaSymbol);
        Assert.Equal("EUR", d.C1);
        Assert.Equal("USD", d.C2);
        Assert.Equal("EURUSD.x", d.Symbol);
        Assert.Equal(1.1m, d.Ratebid);
        Assert.Equal(1.2m, d.Rateask);
        Assert.Equal(D1, d.Lastupdate);
        Assert.True(d.Retired);
        Assert.Equal(101, d.Id);

        var empty = Map<DBRates, Rates>(new DBRates { Id = 5 });
        Assert.Null(empty.MetaSymbol);
        Assert.Null(empty.C1);
        Assert.Equal(default, empty.Lastupdate);
    }

    [Fact]
    public void Rates_To_DBRates()
    {
        var src = new Rates
        {
            MetaSymbol = "M", Symbol = "S", C1 = "a", C2 = "b", Ratebid = 2.1m, Rateask = 2.2m, Lastupdate = D2,
            Retired = true, Id = 102
        };
        var meta = NewDbMeta();
        var d = MapInto(src, new DBRates { Id = 101, Metasymbol = meta });
        Assert.Equal(102, d.Id);
        Assert.Equal("S", d.Symbol);
        Assert.Equal(2.1m, d.Ratebid);
        Assert.Equal(2.2m, d.Rateask);
        Assert.Equal(D2, d.Lastupdate);
        Assert.True(d.Retired);
        Assert.Same(meta, d.Metasymbol);
        Assert.Equal("EURUSD", meta.Name); // ignored
        Assert.Null(Map<Rates, DBRates>(src).Metasymbol);
    }

    // ---------------- Wallet ----------------

    [Fact]
    public void DBWallet_To_Wallet()
    {
        var d = Map<DBWallet, Wallet>(new DBWallet
        {
            Id = 13, Person = NewDbPerson(), Site = new DBSite { Id = 32 }, Name = "wal", Shortname = "w",
            Link = "lnk", Retired = true
        });
        Assert.Equal(12, d.PersonId);
        Assert.Equal(32, d.SiteId);
        Assert.Equal("wal", d.Name);
        Assert.Equal("w", d.Shortname);
        Assert.True(d.Retired);
        Assert.Equal(13, d.Id);
        Assert.Equal(0m, d.Balance);
        Assert.Equal(default, d.Date);
        Assert.Null(d.Accounts);
    }

    [Fact]
    public void Wallet_To_DBWallet()
    {
        var src = new Wallet
        {
            PersonId = 111, SiteId = 112, Name = "n", Shortname = "s", Retired = true, Balance = 5, Date = D2,
            Accounts = new List<Account> { new() }, Id = 113
        };
        var person = NewDbPerson();
        var d = MapInto(src, new DBWallet { Id = 13, Person = person, Link = "keep" });
        Assert.Equal(113, d.Id);
        Assert.Equal("n", d.Name);
        Assert.Equal("s", d.Shortname);
        Assert.True(d.Retired);
        Assert.Equal("keep", d.Link);
        Assert.Same(person, d.Person);
        Assert.Equal(12, person.Id);
        var fresh = Map<Wallet, DBWallet>(src);
        Assert.Null(fresh.Person);
        Assert.Null(fresh.Site);
    }

    // ---------------- Terminal ----------------

    [Fact]
    public void DBTerminal_To_Terminal()
    {
        var d = Map<DBTerminal, Terminal>(NewDbAccount().Terminal);
        Assert.Equal(777L, d.AccountNumber);
        Assert.Equal("br", d.Broker);
        Assert.Equal("fp", d.FullPath);
        Assert.Equal("cb", d.CodeBase);
        Assert.True(d.Retired);
        Assert.True(d.Stopped);
        Assert.Equal("USD", d.Currency);
        Assert.Equal(17, d.Id);

        var empty = Map<DBTerminal, Terminal>(new DBTerminal { Id = 7, Account = new DBAccount() });
        Assert.Equal(0L, empty.AccountNumber);
        Assert.Null(empty.Currency);
    }

    [Fact]
    public void Terminal_And_Jobs_HaveNoReverseMap()
    {
        Assert.Throws<NotSupportedException>(() => Map<Terminal, DBTerminal>(new Terminal()));
        Assert.Throws<NotSupportedException>(() => Map<ScheduledJobView, DBJobs>(new ScheduledJobView()));
    }

    // ---------------- Jobs ----------------

    [Fact]
    public void DBJobs_To_ScheduledJobView()
    {
        var src = new DBJobs
        {
            Id = 121, Classpath = "cp", Grp = "g", Name = "n", Cron = "* *", Description = "d", Statmessage = "sm",
            Prevdate = D1, Nextdate = D2, Params = "p", Disabled = true
        };
        var d = MapInto(src, new ScheduledJobView { IsRunning = true, Retired = true });
        Assert.Equal(D1, d.PrevDate);
        Assert.Equal(D2, d.NextDate);
        Assert.Equal("g", d.Group);
        Assert.Equal("n", d.Name);
        Assert.Equal("* *", d.Schedule);
        Assert.Equal("sm", d.Log);
        Assert.Equal(121, d.Id);
        Assert.True(d.IsRunning); // ignored
        Assert.True(d.Retired); // no source member
        Assert.False(Map<DBJobs, ScheduledJobView>(src).Retired);
    }

    // ---------------- Person ----------------

    [Fact]
    public void DBPerson_To_Person()
    {
        var d = Map<DBPerson, Person>(NewDbPerson());
        Assert.Equal(0, d.CountryId); // ignored
        Assert.Equal(0, d.Languageid); // ignored
        Assert.Equal(D1, d.Created);
        Assert.Equal("cred", d.Credential);
        Assert.Equal("1.2.3.4", d.Regip);
        Assert.Equal("a@b", d.Mail);
        Assert.Equal("adm", d.Privilege);
        Assert.Equal("uuid", d.Uuid);
        Assert.Equal(3, d.Activated);
        Assert.True(d.Retired);
        Assert.Equal(12, d.Id);
    }

    [Fact]
    public void Person_To_DBPerson()
    {
        var src = new Person
        {
            CountryId = 131, Created = D2, Languageid = 132, Credential = "c", Regip = "r", Mail = "m",
            Privilege = "p", Uuid = "u", Activated = 133, Retired = true, Id = 134
        };
        var existing = NewDbPerson();
        var country = existing.Country;
        var d = MapInto(src, existing);
        Assert.Equal(134, d.Id);
        Assert.Equal(D2, d.Created);
        Assert.Equal(132, d.Languageid);
        Assert.Equal("c", d.Credential);
        Assert.Equal("r", d.Regip);
        Assert.Equal("m", d.Mail);
        Assert.Equal("p", d.Privilege);
        Assert.Equal("u", d.Uuid);
        Assert.Equal(133, d.Activated);
        Assert.True(d.Retired);
        Assert.Same(country, d.Country);
        Assert.Equal(31, country.Id); // ignored
        Assert.Null(Map<Person, DBPerson>(src).Country);
    }

    // ---------------- Symbol ----------------

    [Fact]
    public void DBSymbol_To_Symbol()
    {
        var d = Map<DBSymbol, Symbol>(NewDbSymbol());
        Assert.Equal(14, d.MetasymbolId);
        Assert.Equal("EURUSDx", d.Name);
        Assert.Equal("sdesc", d.Description);
        Assert.Equal(D2, d.Expiration);
        Assert.True(d.Retired);
        Assert.Equal(15, d.Id);

        Assert.False(Map<DBSymbol, Symbol>(new DBSymbol { Retired = null }).Retired);
        Assert.False(Map<DBSymbol, Symbol>(new DBSymbol { Retired = 0 }).Retired);
        Assert.True(Map<DBSymbol, Symbol>(new DBSymbol { Retired = -3 }).Retired);
        Assert.Equal(0, Map<DBSymbol, Symbol>(new DBSymbol()).MetasymbolId);
    }

    [Fact]
    public void Symbol_To_DBSymbol_LoadsMetasymbolById()
    {
        var src = new Symbol { MetasymbolId = 141, Name = "n", Description = "d", Expiration = D1, Retired = true, Id = 142 };
        var d = MapInto(src, NewDbSymbol());
        Assert.Equal(141, d.Metasymbol.Id);
        Assert.Null(d.Metasymbol.Name); // freshly loaded entity, not the old one
        Assert.Equal("n", d.Name);
        Assert.Equal("d", d.Description);
        Assert.Equal(D1, d.Expiration);
        Assert.Equal(1, d.Retired);
        Assert.Equal(142, d.Id);

        var d2 = Map<Symbol, DBSymbol>(new Symbol { MetasymbolId = 0, Retired = false });
        Assert.Equal(0, d2.Retired);
        Assert.Null(d2.Expiration);
        Assert.Equal(new[] { (typeof(DBMetasymbol), 141), (typeof(DBMetasymbol), 0) }, lookups);
    }

    // ---------------- Settings ----------------

    [Fact]
    public void DBSettings_To_Settings()
    {
        var d = MapInto(new DBSettings { Id = 151, Propertyname = "p", Value = "v", Description = "d" },
            new Settings { Retired = true });
        Assert.Equal(151, d.Id);
        Assert.Equal("p", d.Propertyname);
        Assert.Equal("v", d.Value);
        Assert.Equal("d", d.Description);
        Assert.True(d.Retired); // no source member
    }

    [Fact]
    public void Settings_To_DBSettings()
    {
        var d = MapInto(new Settings { Id = 152, Retired = true, Propertyname = null, Value = "v2", Description = "d2" },
            new DBSettings { Id = 151, Propertyname = "p", Value = "v", Description = "d" });
        Assert.Equal(152, d.Id);
        Assert.Null(d.Propertyname); // null overwrites
        Assert.Equal("v2", d.Value);
        Assert.Equal("d2", d.Description);
    }

    // ---------------- MetaSymbol (former default: branch) ----------------

    [Fact]
    public void DBMetasymbol_To_MetaSymbol()
    {
        var d = Map<DBMetasymbol, MetaSymbol>(NewDbMeta());
        Assert.Equal(14, d.Id);
        Assert.Equal("EURUSD", d.Name);
        Assert.Equal("md", d.Description);
        Assert.Equal("EUR", d.C1);
        Assert.Equal("USD", d.C2);
        Assert.Equal(2, d.Typ);
        Assert.True(d.Retired);
    }

    [Fact]
    public void MetaSymbol_To_DBMetasymbol()
    {
        var d = Map<MetaSymbol, DBMetasymbol>(new MetaSymbol
            { Id = 161, Name = "n", Description = "d", C1 = "a", C2 = "b", Typ = 3, Retired = true });
        Assert.Equal(161, d.Id);
        Assert.Equal("n", d.Name);
        Assert.Equal("d", d.Description);
        Assert.Equal("a", d.C1);
        Assert.Equal("b", d.C2);
        Assert.Equal(3, d.Typ);
        Assert.True(d.Retired);

        var existing = MapInto(new MetaSymbol { Id = 161, Name = "n", Typ = null }, NewDbMeta());
        Assert.Null(existing.Typ);
        Assert.Null(existing.Description);
        Assert.False(existing.Retired);
    }

    // ---------------- Dispatcher ----------------

    [Fact]
    public void Dispatcher_CoversEveryRegisteredEntityType()
    {
        var pairs = new (Type Db, Type Dto, bool Reverse)[]
        {
            (typeof(DBAccount), typeof(Account), true),
            (typeof(DBTerminal), typeof(Terminal), false),
            (typeof(DBMetasymbol), typeof(MetaSymbol), true),
            (typeof(DBWallet), typeof(Wallet), true),
            (typeof(DBJobs), typeof(ScheduledJobView), false),
            (typeof(DBDeals), typeof(DealInfo), true),
            (typeof(DBRates), typeof(Rates), true),
            (typeof(DBSymbol), typeof(Symbol), true),
            (typeof(DBAccountstate), typeof(AccountState), true),
            (typeof(DBAdviser), typeof(Adviser), true),
            (typeof(DBSettings), typeof(Settings), true),
            (typeof(DBPerson), typeof(Person), true)
        };
        foreach (var (db, dto, reverse) in pairs)
        {
            Assert.True(EntityMapper.CanMap(db, dto), $"{db.Name} -> {dto.Name}");
            Assert.Equal(reverse, EntityMapper.CanMap(dto, db));
        }
    }

    [Fact]
    public void Dispatcher_NullSource()
    {
        Assert.Null(EntityMapper.Map(null, typeof(DBAccount), typeof(Account)));
        var existing = new DBSettings { Id = 1, Value = "v" };
        Assert.Same(existing, EntityMapper.Map(null, existing, typeof(Settings), typeof(DBSettings)));
        Assert.Equal("v", existing.Value);
    }

    private class DBSettingsProxy : DBSettings
    {
    }

    [Fact]
    public void Dispatcher_AcceptsDerivedRuntimeType_LikeNHibernateProxy()
    {
        var d = (Settings)EntityMapper.Map(new DBSettingsProxy { Id = 7, Value = "x" }, typeof(DBSettings),
            typeof(Settings));
        Assert.Equal(7, d.Id);
        Assert.Equal("x", d.Value);
    }
}
