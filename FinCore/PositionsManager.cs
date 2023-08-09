using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using BusinessLogic.BusinessObjects;
using BusinessObjects;
using BusinessObjects.BusinessObjects;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace FinCore;

public class PositionsManager : ITerminalEvents
{
    private static readonly object lockObject = new object();
    private readonly Dictionary<long, Terminal> terminals;
    private readonly Dictionary<long, DealInfo> todayDeals;
    private readonly IMainService xtrade;
    private ConcurrentDictionary<long, PositionInfo> positions;
    private readonly IMessagingService service;
    private readonly RatesService rates;
    private readonly TodayStat todayStat;

    public PositionsManager()
    {
        service = Program.Container.Resolve<IServiceProvider>().GetRequiredService<IMessagingService>();
        xtrade = Program.Container.Resolve<IMainService>();
        rates = Program.Container.Resolve<RatesService>();
        todayDeals = new Dictionary<long, DealInfo>();
        terminals = new Dictionary<long, Terminal>();
        todayStat = new TodayStat();
        fillRiskProps();
        loadPositions();

        foreach (var t in (List<object>) xtrade.GetObjects(EntitiesEnum.Terminal, false))
        {
            var term = (Terminal) t;
            if (term.Retired)
                continue;
            terminals.Add(term.AccountNumber, term);
            if (term.Retired == false)
            {
                var acc = new Account {Number = term.AccountNumber, TerminalId = term.Id};
                todayStat.Accounts.Add(acc);
            }
        }
    }

    public List<PositionInfo> GetAllPositions()
    {
        var result = new List<PositionInfo>();
        lock (lockObject)
        {
            foreach (var posTerm in positions) result.Add(posTerm.Value);
        }
        return result;
    }
    
    public List<PositionInfo> GetPositions4Adviser(long adviserId)
    {
        var result = new List<PositionInfo>();
        lock (lockObject)
        {
            foreach (var posTerm in positions) 
            {
                if (posTerm.Value.Magic == adviserId)
                    result.Add(posTerm.Value);
            }
        }
        return result;
    }

    public void AddOrders(long magicId, long accountNumber, IEnumerable<PositionInfo> orders)
    {
        lock (lockObject)
        {
            bool doSave = false;
            foreach (var order in orders)
            {
                if (positions.TryAdd(order.Ticket, order))
                {
                    InsertPosition(order);
                    doSave = true;
                }
            }

            if (doSave)
                SavePositions();
        }
    }

    public void UpdateOrders(long magicId, long accountNumber, IEnumerable<PositionInfo> orders)
    {
        lock (lockObject)
        {
            bool doSave = false;
            foreach (var order in orders)
            {
                PositionInfo pos = null;
                if (positions.TryGetValue(order.Ticket, out pos))
                {
                    if (positions.TryUpdate(order.Ticket, order, pos))
                    {
                        UpdatePosition(order);
                        doSave = true;
                    }
                }
            }
            if (doSave)
                SavePositions();
        }
    }

    public void DeleteOrders(long magicId, long accountNumber, IEnumerable<PositionInfo> orders)
    {
        lock (lockObject)
        {
            bool doSave = false;
            foreach (var order in orders)
            {
                PositionInfo todel = null;
                if (positions.TryRemove(order.Ticket, out todel))
                {
                    RemovePosition(order.Ticket);
                    doSave = true;
                }
            }
            if (doSave)
                SavePositions();
        }
    }

    public void UpdatePositions(long magicId, long AccountNumber, IEnumerable<PositionInfo> posMagic)
    {
        lock (lockObject)
        {
            var doSave = false;
            var positionsToAdd = new Dictionary<long, PositionInfo>();
            var positionsToDelete = new List<long>();
            Terminal terminal = terminals[AccountNumber];
            foreach (var notcontains in posMagic)
                if (!positionsToAdd.ContainsKey(notcontains.Ticket))
                {
                    notcontains.AccountName = terminal.Broker;
                    notcontains.cur = terminal.Currency;

                    notcontains.Profit = (double) rates.ConvertToUSD(new decimal(notcontains.Profit),
                        notcontains.cur);

                    var acc = todayStat.Accounts.Find(c => c.Number == AccountNumber);
                    if (acc != null && acc.Balance > 0)
                    {
                        double balanceUsd = Convert.ToDouble(rates.ConvertToUSD(acc.Balance, notcontains.cur));
                        notcontains.Value = notcontains.Profit / balanceUsd * 100.0;
                    }
                    else
                        notcontains.Value = notcontains.Profit;

                    
                    // notcontains.Value = (double) rates.ConvertToUSD(new decimal(notcontains.calculateValue()),
                    //    notcontains.cur);
                    
                    positionsToAdd.Add(notcontains.Ticket, notcontains);
                }

            foreach (var pos in positions.Where(x => x.Value.Account.Equals(AccountNumber)))
            {
                var contains = posMagic.Where(x => x.Ticket == pos.Key && x.Account == AccountNumber);
                if (Utils.HasAny(contains))
                {
                    positionsToAdd.Remove(pos.Key);
                    var newvalue = contains.FirstOrDefault();
                    // newvalue.ProfitStopsPercent = pos.Value.ProfitStopsPercent;
                    newvalue.AccountName = terminal.Broker;
                    if (!newvalue.Role.Equals(pos.Value.Role))
                    {
                        newvalue.Role = pos.Value.Role;
                        doSave = true;
                    }

                    if (positions.TryUpdate(pos.Key, newvalue, pos.Value)) UpdatePosition(newvalue);
                }
                else
                {
                    // if (pos.Value.Account == AccountNumber)  (pos.Value.Account == AccountNumber) && (pos.Value.Ticket > 0)
                    if (pos.Value.Account == AccountNumber && pos.Value.Ticket > 0)
                        positionsToDelete.Add(pos.Key);
                }

                foreach (var notcontains in posMagic.Where(x => x.Ticket != pos.Key))
                    if (!positionsToAdd.ContainsKey(notcontains.Ticket))
                    {
                        positionsToAdd.Add(notcontains.Ticket, notcontains);
                        doSave = true;
                    }
            }

            foreach (var toremove in positionsToDelete)
            {
                PositionInfo todel = null;
                if (positions.TryRemove(toremove, out todel))
                {
                    RemovePosition(toremove);
                    doSave = true;
                }
            }

            foreach (var toadd in positionsToAdd)
            {
                toadd.Value.AccountName = terminal.Broker;
                if (positions.TryAdd(toadd.Key, toadd.Value))
                {
                    InsertPosition(toadd.Value);
                    doSave = true;
                }
            }

            if (doSave)
                SavePositions();
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
        if (xtrade == null)
            return null;
        var deals = xtrade.TodayDeals();
        if (Utils.HasAny(deals))
            foreach (var deal in deals)
            {
                if (todayDeals.ContainsKey(deal.Ticket))
                    continue;
                var currency = terminals[deal.Account].Currency;
                deal.Profit = (double) rates.ConvertToUSD(new decimal(deal.Profit), currency);
                if (deal.Commission != 0)
                    deal.Commission = (double)rates.ConvertToUSD(new decimal(deal.Commission), currency);
                if (deal.Swap != 0)
                   deal.Swap = (double)rates.ConvertToUSD(new decimal(deal.Swap), currency);
                todayDeals.Add(deal.Ticket, deal);
            }

        var now = DateTime.UtcNow;
        var toDelete = new List<long>();
        foreach (var val in todayDeals)
        {
            var time = DateTime.Parse(val.Value.CloseTime);
            if (!Utils.IsSameDay(time, now))
                toDelete.Add(val.Key);
        }
        foreach (var val in toDelete) todayDeals.Remove(val);
        return todayDeals.Values.OrderByDescending(x => x.CloseTime).ToList();
    }

    
    public TodayStat GetTodayStat()
    {
        todayStat.Deals = GetTodayDeals();
        // reset profits
        todayStat.Accounts.ForEach(c =>
        {
            c.DailyProfit = 0;
            c.DailyMaxGain = 0;
            c.StopTrading = false;
        });
        double sumReal = 0;
        foreach (var deal in todayDeals.OrderBy(c => c.Value.CloseTime))
        {
            sumReal += deal.Value.Profit;
            var acc = todayStat.Accounts.Find(c => c.Number == deal.Value.Account);
            if (acc != null)
            {
                acc.DailyProfit += new decimal(deal.Value.Profit);
                if (acc.DailyProfit > 0)
                    acc.DailyMaxGain = Math.Max(acc.DailyMaxGain, acc.DailyProfit);
            }
        }

        foreach (var deal in todayDeals)
            deal.Value.AccountName = AccountRiskInfo(deal.Value.Account, terminals[deal.Value.Account].Broker);
        todayStat.TodayGainReal = decimal.Round((decimal) sumReal, 2);
        // UpdateRiskManager();
        return todayStat;
    }

    public void UpdateBalance(int AccountNumber, decimal Balance, decimal Equity)
    {
        var acc = todayStat.Accounts.Find(c => c.Number == AccountNumber);
        if (acc != null)
        {
            acc.Balance = Balance;
            acc.Equity = Equity;
        }
    }

    public bool CheckTradeAllowed(SignalInfo signal)
    {
        var acc = todayStat.Accounts.Find(c => c.Number == signal.ObjectId);
        if (acc != null)
        {
            var balance = new decimal(0);
            decimal.TryParse((string) signal.Data, out balance);
            if (balance > 0)
                acc.Balance = balance;
            CheckRiskForAccount(ref acc);
            signal.Value = acc.StopTrading ? 1 : 0;
            signal.SetData(acc.StopReason);
            if (acc.StopTrading && !string.IsNullOrEmpty(acc.StopReason))
            {
                var log = Program.Container.Resolve<IWebLog>();
                if (log != null)
                    log.Log(acc.StopReason);
            }

            return acc.StopTrading;
        }

        return true;
    }

    public void DeletePosition(long Ticket)
    {
        lock (lockObject)
        {
            PositionInfo todel = null;
            if (positions.TryRemove(Ticket, out todel))
                RemovePosition(Ticket);
        }
    }

    private void loadPositions()
    {
        try
        {
            positions = new ConcurrentDictionary<long, PositionInfo>();
            var pos = xtrade.GetGlobalProp(xtradeConstants.SETTINGS_PROPERTY_POSITIONS);
            if (!string.IsNullOrEmpty(pos))
                positions = JsonConvert.DeserializeObject<ConcurrentDictionary<long, PositionInfo>>(pos);
        }
        catch (Exception)
        {
            positions = new ConcurrentDictionary<long, PositionInfo>();
        }
    }

    private void fillRiskProps()
    {
        var riskPerDay = xtrade.GetGlobalProp(xtradeConstants.SETTINGS_PROPERTY_RISK_PER_DAY);
        var dRiskPerDay = new decimal(0.02);
        decimal.TryParse(riskPerDay, out dRiskPerDay);
        todayStat.RISK_PER_DAY = dRiskPerDay;

        var riskDailyMinGain = xtrade.GetGlobalProp(xtradeConstants.SETTINGS_PROPERTY_RISK_DAILY_MIN_GAIN);
        var dRiskDailyMinGain = new decimal(0.007);
        decimal.TryParse(riskDailyMinGain, out dRiskDailyMinGain);
        todayStat.DAILY_MIN_GAIN = dRiskDailyMinGain;

        var riskDailyLossAfterGain =
            xtrade.GetGlobalProp(xtradeConstants.SETTINGS_PROPERY_RISK_DAILY_LOSS_AFTER_GAIN);
        var dRiskDailyLossAfterGain = new decimal(0.3);
        decimal.TryParse(riskDailyLossAfterGain, out dRiskDailyLossAfterGain);
        todayStat.DAILY_LOSS_AFTER_GAIN = dRiskDailyLossAfterGain;
    }

    protected void SavePositions()
    {
        xtrade.SetGlobalProp(xtradeConstants.SETTINGS_PROPERTY_POSITIONS, JsonConvert.SerializeObject(positions));
    }

    private string AccountRiskInfo(long AccountNumber, string AccountName)
    {
        var res = new StringBuilder(AccountName);
        var acc = todayStat.Accounts.Find(c => c.Number == AccountNumber);
        if (acc != null)
        {
            CheckRiskForAccount(ref acc);
            res.Append(string.Format(" {0:0.##},{1:0.##},", acc.DailyProfit, acc.DailyMaxGain));
            if (acc.StopTrading)
                //if (!String.IsNullOrEmpty(acc.StopReason))
                //    res.Append(acc.StopReason);
                res.Append("Blocked");
            else
                res.Append("Allowed");
        }

        return res.ToString();
    }

    private void CheckRiskForAccount(ref Account acc)
    {
        var dow = DateTime.Now.DayOfWeek;
        // || (dow == DayOfWeek.Wednesday)
        //if (dow == DayOfWeek.Sunday || dow == DayOfWeek.Saturday)
        //{
        //    acc.StopReason = "Today is non trading day of the week: " + dow + "!!! RELAX!";
        //    acc.StopTrading = true;
        //    return;
        //}

        //var allowedLoss = todayStat.RISK_PER_DAY * acc.Balance;
        var startBalance = acc.Balance; //allowedLoss +
        if (startBalance <= 0) {
            acc.StopReason = string.Format("Account [{0}] has zero balance!", acc.Number);
            acc.StopTrading = true;
            return;
        }
        /* if (acc.DailyProfit < 0)
        {
            decimal currentDailyRisk = Math.Abs(acc.DailyProfit) / startBalance;
            if (currentDailyRisk > todayStat.RISK_PER_DAY)  // GAME OVER FOR TODAY
            {
                acc.StopReason = string.Format("[{0}] ", acc.Number);
                acc.StopReason += string.Format("GAME OVER FOR TODAY! Risk too high for today = {0:0.##}%! RELAX.", currentDailyRisk * 100);
                acc.StopTrading = true;
                return;
            }
        }
        decimal dailyMinGain = todayStat.DAILY_MIN_GAIN * startBalance;
        if ((acc.DailyProfit > 0) && (acc.DailyMaxGain > dailyMinGain) && (acc.DailyMaxGain > 0)) // Check to save what we alredy earned today
        {
            decimal gainDifferencePercent = (acc.DailyMaxGain - acc.DailyProfit) / acc.DailyMaxGain;
            if (gainDifferencePercent >= todayStat.DAILY_LOSS_AFTER_GAIN)
            {
                // ENOUGH PLAY FOR TODAY
                acc.StopReason = string.Format("[{0}] ", acc.Number);
                acc.StopReason += string.Format("ENOUGH PLAY FOR TODAY! YOU ALREADY EARNED = {0:0.##}! IT IS ENOUGH. RELAX.", acc.DailyProfit);
                acc.StopTrading = true;
                return;
            }
        } */
        acc.StopTrading = false;
    }

    public void UpdateRiskManager()
    {
        //IWebLog log = Program.Container.Resolve<IWebLog>();
        todayStat.Accounts.ForEach(c => CheckRiskForAccount(ref c));
    }

    #region Interface Imp

    public void InsertPosition(PositionInfo pos)
    {
        service.SendMessage(WsMessageType.InsertPosition, pos);
    }

    public void UpdatePosition(PositionInfo pos)
    {
        service.SendMessage(WsMessageType.UpdatePosition, pos);
    }

    public void UpdatePositionFromClient(PositionInfo pos)
    {
        if (positions.ContainsKey(pos.Ticket)) positions[pos.Ticket].Role = pos.Role;
    }

    public void RemovePosition(long Ticket)
    {
        service.SendMessage(WsMessageType.RemovePosition, Ticket);
    }

    #endregion
}
