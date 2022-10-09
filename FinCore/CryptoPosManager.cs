using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using BusinessLogic;
using BusinessLogic.BusinessObjects;
using BusinessObjects;
using BusinessObjects.BusinessObjects;
using CryptoExchange.Net.Authentication;
using FTX.Net.Clients;
using FTX.Net.Enums;
using FTX.Net.Objects;
using Kucoin.Net.Clients;
using Kucoin.Net.Objects;
using Kucoin.Net.Objects.Models.Futures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderStatus = Kucoin.Net.Enums.OrderStatus;

namespace FinCore;

public class CryptoPosManager : ITerminalEvents
{
    private static readonly object lockObject = new object();
    private readonly Dictionary<long, Terminal> terminals;
    private readonly Dictionary<long, DealInfo> todayDeals;
    private readonly IMainService xtrade;
    private ConcurrentDictionary<long, PositionInfo> positions;
    private readonly IMessagingService service;
    private readonly RatesService rates;
    private readonly TodayStat todayStat;
    private static IWebLog log;
    private static Account ftxAccount;
    private static Account kucoinAccount;
    
    public CryptoPosManager()
    {
        service = Program.Container.Resolve<IServiceProvider>().GetRequiredService<IMessagingService>();
        
        log = Program.Container.Resolve<IWebLog>();
        xtrade = Program.Container.Resolve<IMainService>();
        rates = Program.Container.Resolve<RatesService>();

        todayDeals = new Dictionary<long, DealInfo>();
        terminals = new Dictionary<long, Terminal>();
        todayStat = new TodayStat();

        LoadAccounts();
        LoadPositions();
    }

    private void LoadAccounts()
    {
        var acc = (List<object>) xtrade.GetObjects(EntitiesEnum.Account, false);
        foreach (var item in acc)
        {
            Account account = (Account) item;
            if (account != null && account.Description.Contains("FTX"))
            {
                ftxAccount = account;
                break;
            }

        }

        foreach (var item in acc)
        {
            Account account = (Account) item;
            if (account != null && account.Description.Contains("KuCoin"))
            {
                kucoinAccount = account;
                break;
            }
        }
    }

    private bool AssignAccountAndAdd(Account acc, PositionInfo pos)
    {
        if (acc != null)
        {
            pos.Account = acc.Id;
            pos.AccountName = acc.Description;
        }
        return positions.TryAdd(pos.Ticket, pos);
    }

    protected void LoadPositions()
    {
        positions = new ConcurrentDictionary<long, PositionInfo>();
        LoadPositionsFtx().GetAwaiter().GetResult();
        LoadPositionsKuCoin().GetAwaiter().GetResult();
    }
    
    public List<PositionInfo> GetAllPositions()
    {
        LoadPositions();
        var result = new List<PositionInfo>();
        lock (lockObject)
        {
            foreach (var posTerm in positions) result.Add(posTerm.Value);
        }
        return result;
    }

    #region FTXAPI

    private FTXClient _ftxClient;
    private bool initFtx()
    {
        if (_ftxClient == null)
        { 
            var conf = XTradeConfig.Self();
            _ftxClient = new FTXClient(new FTXClientOptions()
            {
                ApiCredentials = new ApiCredentials(conf.FTXAPIKey, conf.FTXAPISecret),
                LogLevel = LogLevel.Trace,
                RequestTimeout = TimeSpan.FromSeconds(60)
            });
        }
        return _ftxClient != null;
    }
    
    private async Task<bool> LoadPositionsFtx()
    {
        try
        {
            if (!initFtx())
                return false;
            long id = 0;

            var accountDataWP = await _ftxClient.TradeApi.Account.GetPositionsAsync(false);
            if (accountDataWP.Success)
            {     
                foreach (var p in accountDataWP.Data)
                {
                    if (!p.EntryPrice.HasValue)
                    {
                        continue;
                    }
                    var pos = new PositionInfo();
                    pos.Lots = Convert.ToDouble(p.Quantity);
                    pos.Symbol = p.Future;
                    pos.Type = (int) p.Side; 
                    pos.Ticket = id++;
                    pos.Profit =  Convert.ToDouble(p.RecentPnl);
                    pos.Openprice = Convert.ToDouble(p.EntryPrice);
                    pos.Role = "RegularTrail";
                    AssignAccountAndAdd(ftxAccount, pos);
                }
            }
            
            var accountData = await _ftxClient.TradeApi.Trading.GetOpenOrdersAsync();
            if (accountData.Success)
            {
                foreach (var p in accountData.Data)
                {
                    var pos = new PositionInfo();
                    pos.Lots = Convert.ToDouble(p.Quantity);
                    pos.Symbol = p.Future;
                    pos.Type = (int) p.Side; 
                    pos.Ticket = p.Id;
                    
                    if (p.Type == OrderType.Limit)
                        pos.Role = "PendingLimit";
                    else
                        pos.Role = "RegularTrail";
                    pos.Openprice = Convert.ToDouble(p.Price);

                    AssignAccountAndAdd(ftxAccount, pos);
                }
            }
            
            var accountDataP = await _ftxClient.TradeApi.Trading.GetTriggerOrdersAsync();
            if (accountDataP.Success)
            {     
                foreach (var p in accountDataP.Data)
                {
                    if (p.Status != TriggerOrderStatus.Open)
                    {
                        continue;
                    }
                    var pos = new PositionInfo();
                    pos.Lots = Convert.ToDouble(p.Quantity);
                    pos.Symbol = p.Future;
                    pos.Type = (int) p.Side; 
                    pos.Ticket = p.Id;
                    
                    if (p.Type == OrderType.Limit)
                        pos.Role = "PendingLimit";
                    else
                        pos.Role = "RegularTrail";

                    if (p.TriggerType == TriggerOrderType.Stop)
                        pos.Role = "PendingStopMarket";
                    if (p.TriggerType == TriggerOrderType.TakeProfit)
                        pos.Role = "PendingStopMarket";
                    
                    pos.Openprice = Convert.ToDouble(p.TriggerPrice);
                    AssignAccountAndAdd(ftxAccount, pos);
                }
            }

            return true;
        }
        catch (Exception e)
        {
            log?.Error(e.ToString());
            return false;
        }
    }

    #endregion
    
    // KuCoin
    #region KuCoinAPI 
    
    private KucoinClient _kucoinClient;

    private bool initKuCoin()
    {
        if (_kucoinClient == null)
        {
            var conf = XTradeConfig.Self();
            _kucoinClient = new KucoinClient(new KucoinClientOptions()
            {
                ApiCredentials = new KucoinApiCredentials(conf.KuCoinAPIKey, conf.KuCoinAPISecret, conf.KuCoinPassPhrase),
                LogLevel = LogLevel.Trace,
                RequestTimeout = TimeSpan.FromSeconds(60),
                FuturesApiOptions = new KucoinRestApiClientOptions
                {
                    ApiCredentials = new KucoinApiCredentials(conf.KuCoinFutureAPIKey, conf.KuCoinFutureAPISecret, conf.KuCoinFuturePassPhrase),
                    AutoTimestamp = false
                }
            });
        }
        return _kucoinClient != null;
    }

    private string KuCointOrder2String(KucoinFuturesOrder p)
    {
        switch (p.Type)
        {
            case Kucoin.Net.Enums.OrderType.Limit:
                return "PendingLimit";
            case Kucoin.Net.Enums.OrderType.LimitStop:
                return "PendingLimitStop";
            case Kucoin.Net.Enums.OrderType.Stop:
                return "PendingStop";
            case Kucoin.Net.Enums.OrderType.Market:
            {
                if (p.CloseOrder)
                {
                    return "PendingStopMarket";
                }
                return "RegularTrail";
            }
            case Kucoin.Net.Enums.OrderType.MarketStop:
                return "PendingStopMarket";
        }
        return "RegularTrail";
    }
    
    private async Task<bool> LoadPositionsKuCoin()
    {
        try
        {
            if (!initKuCoin())
                return false;
            long id = 100;

            var accountDataWP = await _kucoinClient.FuturesApi.Account.GetPositionsAsync();
            if (accountDataWP.Success)
            {     
                foreach (var p in accountDataWP.Data)
                {
                    var pos = new PositionInfo();
                    pos.Lots = Math.Abs(Convert.ToDouble(p.CurrentQuantity));
                    pos.Symbol = p.Symbol;
                    pos.Type = (p.CurrentQuantity > 0)?0:1;
                    pos.Ticket = id++; 
                    pos.Profit =  Convert.ToDouble(p.UnrealizedPnl);
                    pos.Openprice = Convert.ToDouble(p.AverageEntryPrice);
                    pos.Role = "RegularTrail";
                    AssignAccountAndAdd(kucoinAccount, pos);
                }
            }

            var accountData = await _kucoinClient.FuturesApi.Trading.GetOrdersAsync(null, OrderStatus.Active);
            if (accountData.Success)
            {
                foreach (var p in accountData.Data.Items)
                {
                    var pos = new PositionInfo();
                    pos.Lots = Convert.ToDouble(p.Quantity);
                    pos.Symbol = p.Symbol;
                    pos.Type = (int) p.Side;
                    pos.Ticket = id++;

                    pos.Role = KuCointOrder2String(p);
                    pos.Openprice = Convert.ToDouble(p.Price);
                    AssignAccountAndAdd(kucoinAccount, pos);
                }
            }

            var accountDataP = await _kucoinClient.FuturesApi.Trading.GetUntriggeredStopOrdersAsync();
            if (accountDataP.Success)
            {     
                foreach (var p in accountDataP.Data.Items)
                {
                    if (!p.CloseOrder)
                        continue;
                    var pos = new PositionInfo();
                    pos.Lots = Convert.ToDouble(p.Quantity);
                    pos.Symbol = p.Symbol;
                    pos.Type = (int) p.Side; 
                    pos.Ticket = id++;
                    
                    pos.Role = KuCointOrder2String(p);
                    pos.Openprice = Convert.ToDouble(p.StopPrice);
                    AssignAccountAndAdd(kucoinAccount, pos);
                }
            }

            return true;
        }
        catch (Exception e)
        {
            log?.Error(e.ToString());
            return false;
        }
    }

    

    
    #endregion // KuCoin

    public List<PositionInfo> GetPositions4Adviser(long adviserId)
    {
        throw new NotImplementedException();
    }
    
    public void AddOrders(long magicId, long accountNumber, IEnumerable<PositionInfo> orders)
    {
        lock (lockObject)
        {
            //bool doSave = false;
            foreach (var order in orders)
            {
                if (positions.TryAdd(order.Ticket, order))
                {
                    InsertPosition(order);
                    //doSave = true;
                }
            }

            //if (doSave)
            //    SavePositions();
        }
    }

    public void UpdateOrders(long magicId, long accountNumber, IEnumerable<PositionInfo> orders)
    {
        lock (lockObject)
        {
            //bool doSave = false;
            foreach (var order in orders)
            {
                PositionInfo pos = null;
                if (positions.TryGetValue(order.Ticket, out pos))
                {
                    if (positions.TryUpdate(order.Ticket, order, pos))
                    {
                        UpdatePosition(order);
                        //doSave = true;
                    }
                }
            }
        }
    }

    public void DeleteOrders(long magicId, long accountNumber, IEnumerable<PositionInfo> orders)
    {
        lock (lockObject)
        {
            //bool doSave = false;
            foreach (var order in orders)
            {
                PositionInfo todel = null;
                if (positions.TryRemove(order.Ticket, out todel))
                {
                    RemovePosition(order.Ticket);
                    //doSave = true;
                }
            }
            //if (doSave)
            //    SavePositions();
        }
    }

    public void UpdatePositions(long magicId, long AccountNumber, IEnumerable<PositionInfo> posMagic)
    {
        lock (lockObject)
        {
            //var doSave = false;
            var positionsToAdd = new Dictionary<long, PositionInfo>();
            var positionsToDelete = new List<long>();
            foreach (var notcontains in posMagic)
                if (!positionsToAdd.ContainsKey(notcontains.Ticket))
                {
                    notcontains.AccountName = terminals[AccountNumber].Broker;
                    notcontains.Profit = (double) rates.ConvertToUSD(new decimal(notcontains.Profit),
                        terminals[AccountNumber].Currency);
                    notcontains.Value = (double) rates.ConvertToUSD(new decimal(notcontains.calculateValue()),
                        notcontains.cur); // + notcontains.Profit;
                    positionsToAdd.Add(notcontains.Ticket, notcontains);
                }

            foreach (var pos in positions.Where(x => x.Value.Account.Equals(AccountNumber)))
            {
                var contains = posMagic.Where(x => x.Ticket == pos.Key && x.Account == AccountNumber);
                if (Utils.HasAny(contains))
                {
                    positionsToAdd.Remove(pos.Key);
                    var newvalue = contains.FirstOrDefault();
                    //newvalue.ProfitStopsPercent = pos.Value.ProfitStopsPercent;
                    newvalue.AccountName = terminals[AccountNumber].Broker;
                    if (!newvalue.Role.Equals(pos.Value.Role))
                    {
                        newvalue.Role = pos.Value.Role;
                        //doSave = true;
                    }

                    if (positions.TryUpdate(pos.Key, newvalue, pos.Value)) UpdatePosition(newvalue);
                }
                else
                {
                    //if (pos.Value.Account == AccountNumber)  (pos.Value.Account == AccountNumber) && (pos.Value.Ticket > 0)
                    if (pos.Value.Account == AccountNumber && pos.Value.Ticket > 0)
                        positionsToDelete.Add(pos.Key);
                }

                foreach (var notcontains in posMagic.Where(x => x.Ticket != pos.Key))
                    if (!positionsToAdd.ContainsKey(notcontains.Ticket))
                    {
                        positionsToAdd.Add(notcontains.Ticket, notcontains);
                        //doSave = true;
                    }
            }

            foreach (var toremove in positionsToDelete)
            {
                PositionInfo todel = null;
                if (positions.TryRemove(toremove, out todel))
                {
                    RemovePosition(toremove);
                    //doSave = true;
                }
            }

            foreach (var toadd in positionsToAdd)
            {
                toadd.Value.AccountName = terminals[AccountNumber].Broker;
                if (positions.TryAdd(toadd.Key, toadd.Value))
                {
                    InsertPosition(toadd.Value);
                    //doSave = true;
                }
            }

            //if (doSave)
            //    SavePositions();
        }
    }

    public PositionInfo getPosition(long ticket)
    {
        return positions[(int)ticket];
    }

    public void UpdateSLTP(long magicId, long AccountNumber, IEnumerable<PositionInfo> UpdatePositions)
    {
        lock (lockObject)
        {
            var positionsToAdd = new Dictionary<long, PositionInfo>();
            var positionsToDelete = new List<long>();
            foreach (var notcontains in UpdatePositions)
                if (!positionsToAdd.ContainsKey(notcontains.Ticket))
                {
                    notcontains.AccountName = terminals[AccountNumber].Broker;
                    notcontains.Profit = (double) rates.ConvertToUSD(new decimal(notcontains.Profit),
                        terminals[AccountNumber].Currency);
                    positionsToAdd.Add(notcontains.Ticket, notcontains);
                }

            foreach (var pos in positions.Where(x => x.Value.Account.Equals(AccountNumber)))
            {
                var contains = UpdatePositions.Where(x => x.Ticket == pos.Key && x.Account == AccountNumber);
                if (contains != null && contains.Count() > 0)
                {
                    positionsToAdd.Remove(pos.Key);
                    var newvalue = contains.FirstOrDefault();
                    newvalue.AccountName = terminals[AccountNumber].Broker;
                    if (positions.TryUpdate(pos.Key, newvalue, pos.Value)) UpdatePosition(newvalue);
                }
                else
                {
                    //if (pos.Value.Magic == magicId) (pos.Value.Account == AccountNumber)
                    if (pos.Value.Magic == magicId) //&& (pos.Value.Ticket > 0)
                        positionsToDelete.Add(pos.Key);
                }

                foreach (var notcontains in UpdatePositions.Where(x => x.Ticket != pos.Key))
                    if (!positionsToAdd.ContainsKey(notcontains.Ticket))
                        positionsToAdd.Add(notcontains.Ticket, notcontains);
            }

            foreach (var toremove in positionsToDelete)
            {
                PositionInfo todel = null;
                if (positions.TryRemove(toremove, out todel)) RemovePosition(toremove);
            }

            foreach (var toadd in positionsToAdd)
            {
                toadd.Value.AccountName = terminals[AccountNumber].Broker;
                if (positions.TryAdd(toadd.Key, toadd.Value)) InsertPosition(toadd.Value);
            }
        }
    }

    public List<DealInfo> GetTodayDeals()
    {
        List<DealInfo> dealInfos = new List<DealInfo>();
        // TODO: implement deals
        return dealInfos;
    }

    public TodayStat GetTodayStat()
    {

        return todayStat;
    }

    public bool CheckTradeAllowed(SignalInfo signal)
    {
        throw new NotImplementedException();
    }

    public void UpdateBalance(int AccountNumber, decimal Balance, decimal Equity)
    {
   
    }
    
    public void DeletePosition(long Ticket)
    {
   
    }
    

    #region Interface Imp

    public void InsertPosition(PositionInfo pos)
    {
        //service.SendMessage(WsMessageType.InsertPosition, pos);
    }

    public void UpdatePosition(PositionInfo pos)
    {
        service.SendMessage(WsMessageType.UpdatePosition, pos);
    }

    public void UpdatePositionFromClient(PositionInfo pos)
    {
        lock (lockObject)
        {
            if (positions.ContainsKey(pos.Ticket))
                positions[pos.Ticket].Role = pos.Role;
        }
    }

    public void RemovePosition(long Ticket)
    {
        // service.SendMessage(WsMessageType.RemovePosition, Ticket);
    }

    #endregion
}
