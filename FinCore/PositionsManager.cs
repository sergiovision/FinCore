using Autofac;
using BusinessObjects;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinCore
{
    public class PositionsManager : ITerminalEvents
    {
        private static readonly object lockObject = new object();
        private ConcurrentDictionary<long, PositionInfo> positions;
        private readonly Dictionary<long, Terminal> terminals;
        private readonly Dictionary<long, DealInfo> todayDeals;
        private TodayStat todayStat;
        private readonly IMainService xtrade;
        private IMessagingService service;

        public PositionsManager()
        {
            service = Program.Container.Resolve<IServiceProvider>().GetRequiredService<IMessagingService>();

            xtrade = Program.Container.Resolve<IMainService>();
            todayDeals = new Dictionary<long, DealInfo>();
            terminals = new Dictionary<long, Terminal>();
            todayStat = new TodayStat();
            fillRiskProps();
            loadPositions();

            foreach (object t in (List<object>)xtrade.GetObjects(EntitiesEnum.Terminal))
            {
                Terminal term = (Terminal)t;
                if (term.Retired)
                    continue;
                terminals.Add(term.AccountNumber, term);
                if (term.Retired == false)
                {
                    var acc = new Account();
                    acc.Number = term.AccountNumber;
                    acc.TerminalId = term.Id;
                    todayStat.Accounts.Add(acc);
                }
            }
        }

        protected void loadPositions()
        {
            try
            {
                positions = new ConcurrentDictionary<long, PositionInfo>();
                string pos = xtrade.GetGlobalProp(xtradeConstants.SETTINGS_PROPERTY_POSITIONS);
                if (!String.IsNullOrEmpty(pos))
                    positions = JsonConvert.DeserializeObject<ConcurrentDictionary<long, PositionInfo>>(pos);

            }
            catch (Exception)
            {
                positions = new ConcurrentDictionary<long, PositionInfo>();
            }
        }

        protected void fillRiskProps()
        {
            string riskPerDay = xtrade.GetGlobalProp(xtradeConstants.SETTINGS_PROPERTY_RISK_PER_DAY);
            decimal dRiskPerDay = new decimal(0.02);
            decimal.TryParse(riskPerDay, out dRiskPerDay);
            todayStat.RISK_PER_DAY = dRiskPerDay;

            string riskDailyMinGain = xtrade.GetGlobalProp(xtradeConstants.SETTINGS_PROPERTY_RISK_DAILY_MIN_GAIN);
            decimal dRiskDailyMinGain = new decimal(0.007);
            decimal.TryParse(riskDailyMinGain, out dRiskDailyMinGain);
            todayStat.DAILY_MIN_GAIN = dRiskDailyMinGain;

            string riskDailyLossAfterGain = xtrade.GetGlobalProp(xtradeConstants.SETTINGS_PROPERY_RISK_DAILY_LOSS_AFTER_GAIN);
            decimal dRiskDailyLossAfterGain = new decimal(0.3);
            decimal.TryParse(riskDailyLossAfterGain, out dRiskDailyLossAfterGain);
            todayStat.DAILY_LOSS_AFTER_GAIN = dRiskDailyLossAfterGain;
        }

        public List<PositionInfo> GetAllPositions()
        {
            List<PositionInfo> result = new List<PositionInfo>();
            lock (lockObject)
            {
                foreach (var posTerm in positions) result.Add(posTerm.Value);
            }
            return result;
        }

        public void UpdatePositions(long magicId, long AccountNumber, IEnumerable<PositionInfo> posMagic)
        {
            lock (lockObject)
            {
                bool doSave = false;
                Dictionary<long, PositionInfo> positionsToAdd = new Dictionary<long, PositionInfo>();
                List<long> positionsToDelete = new List<long>();
                foreach (var notcontains in posMagic)
                    if (!positionsToAdd.ContainsKey(notcontains.Ticket))
                    {
                        notcontains.AccountName = terminals[AccountNumber].Broker;
                        notcontains.Profit = (double)xtrade.ConvertToUSD(new decimal(notcontains.Profit), terminals[AccountNumber].Currency);
                        notcontains.Value = (double)xtrade.ConvertToUSD(new decimal(notcontains.calculateValue()), notcontains.cur);// + notcontains.Profit;
                        positionsToAdd.Add(notcontains.Ticket, notcontains);
                    }

                foreach (var pos in positions.Where(x => x.Value.Account.Equals(AccountNumber)))
                {
                    var contains = posMagic.Where(x => x.Ticket == pos.Key && x.Account == AccountNumber);
                    if (contains != null && contains.Count() > 0)
                    {
                        positionsToAdd.Remove(pos.Key);
                        var newvalue = contains.FirstOrDefault();
                        //newvalue.ProfitStopsPercent = pos.Value.ProfitStopsPercent;
                        newvalue.AccountName = terminals[AccountNumber].Broker;
                        if (!newvalue.Role.Equals(pos.Value.Role))
                        {
                            newvalue.Role = pos.Value.Role;
                            doSave = true;
                        }
                        if (positions.TryUpdate(pos.Key, newvalue, pos.Value))
                        {
                            UpdatePosition(newvalue);
                        }
                    }
                    else
                    {
                        //if (pos.Value.Account == AccountNumber)  (pos.Value.Account == AccountNumber) && (pos.Value.Ticket > 0)
                        if ((pos.Value.Account == AccountNumber) && (pos.Value.Ticket > 0))
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
                    toadd.Value.AccountName = terminals[AccountNumber].Broker;
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

        protected void SavePositions()
        {
            xtrade.SetGlobalProp(xtradeConstants.SETTINGS_PROPERTY_POSITIONS, JsonConvert.SerializeObject(positions));
        }

        public void UpdateSLTP(long magicId, long AccountNumber, IEnumerable<PositionInfo> UpdatePositions)
        {
            lock (lockObject)
            {
                Dictionary<long, PositionInfo> positionsToAdd = new Dictionary<long, PositionInfo>();
                List<long> positionsToDelete = new List<long>();
                foreach (var notcontains in UpdatePositions)
                    if (!positionsToAdd.ContainsKey(notcontains.Ticket))
                    {
                        notcontains.AccountName = terminals[AccountNumber].Broker;
                        notcontains.Profit = (double)xtrade.ConvertToUSD(new decimal(notcontains.Profit), terminals[AccountNumber].Currency);
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
                        if (positions.TryUpdate(pos.Key, newvalue, pos.Value))
                        {
                            UpdatePosition(newvalue);
                        }
                    }
                    else
                    {
                        //if (pos.Value.Magic == magicId) (pos.Value.Account == AccountNumber)
                        if ((pos.Value.Magic == magicId))//&& (pos.Value.Ticket > 0)
                            positionsToDelete.Add(pos.Key);
                    }

                    foreach (var notcontains in UpdatePositions.Where(x => x.Ticket != pos.Key))
                        if (!positionsToAdd.ContainsKey(notcontains.Ticket))
                            positionsToAdd.Add(notcontains.Ticket, notcontains);
                }

                foreach (var toremove in positionsToDelete)
                {
                    PositionInfo todel = null;
                    if (positions.TryRemove(toremove, out todel))
                    {
                        RemovePosition(toremove);
                    }
                }

                foreach (var toadd in positionsToAdd)
                {
                    toadd.Value.AccountName = terminals[AccountNumber].Broker;
                    if (positions.TryAdd(toadd.Key, toadd.Value))
                    {
                        InsertPosition(toadd.Value);
                    }
                }
            }
        }

        public List<DealInfo> GetTodayDeals()
        {
            var xtrade = Program.Container.Resolve<IMainService>();
            if (xtrade == null)
                return null;
            var deals = xtrade.TodayDeals();
            if (deals != null)
            {
                foreach (var deal in deals)
                {
                    if (todayDeals.ContainsKey(deal.Ticket))
                        continue;
                    string currency = terminals[deal.Account].Currency;
                    deal.Profit = (double)xtrade.ConvertToUSD(new decimal(deal.Profit), currency);
                    bool IsDemo = terminals[deal.Account].Demo;
                    todayDeals.Add(deal.Ticket, deal);
                }
            }
            DateTime now = DateTime.UtcNow;
            List<long> toDelete = new List<long>();
            foreach (var val in todayDeals)
            {
                DateTime time = DateTime.Parse(val.Value.CloseTime);
                if (now.DayOfYear != time.DayOfYear)
                {
                    toDelete.Add(val.Key);
                }
            }
            foreach (var val in toDelete)
            {
                todayDeals.Remove(val);
            }
            return todayDeals.Values.OrderByDescending(x => x.CloseTime).ToList();
        }

        protected string AccountRiskInfo(long AccountNumber, string AccountName)
        {
            StringBuilder res = new StringBuilder(AccountName);
            var acc = todayStat.Accounts.Find(c => (c.Number == AccountNumber));
            if (acc != null)
            {
                CheckRiskForAccount(ref acc);
                res.Append(string.Format(" {0:0.##},{1:0.##},", acc.DailyProfit, acc.DailyMaxGain));
                if (acc.StopTrading)
                {
                    //if (!String.IsNullOrEmpty(acc.StopReason))
                    //    res.Append(acc.StopReason);
                    res.Append("Blocked");
                }
                else
                    res.Append("Allowed");
            }
            return res.ToString();
        }

        public TodayStat GetTodayStat()
        {
            todayStat.Deals = GetTodayDeals();
            // reset profits
            todayStat.Accounts.ForEach(c => { c.DailyProfit = 0; c.DailyMaxGain = 0; c.StopTrading = false; });
            double sumReal = 0;
            double sumDemo = 0;
            foreach (var deal in todayDeals.OrderBy(c => c.Value.CloseTime))
            {
                bool IsDemo = terminals[deal.Value.Account].Demo;
                if (IsDemo)
                    sumDemo += deal.Value.Profit;
                else
                    sumReal += deal.Value.Profit;
                var acc = todayStat.Accounts.Find(c => (c.Number == deal.Value.Account));
                if (acc != null)
                {
                    acc.DailyProfit += new decimal(deal.Value.Profit);
                    if (acc.DailyProfit > 0)
                        acc.DailyMaxGain = Math.Max(acc.DailyMaxGain, acc.DailyProfit);
                }
            }
            foreach (var deal in todayDeals)
            {
                deal.Value.AccountName = AccountRiskInfo(deal.Value.Account, terminals[deal.Value.Account].Broker);
            }
            todayStat.TodayGainDemo = decimal.Round((decimal)sumDemo, 2);
            todayStat.TodayGainReal = decimal.Round((decimal)sumReal, 2);
            // UpdateRiskManager();
            return todayStat;
        }

        public void UpdateBalance(int AccountNumber, decimal Balance, decimal Equity)
        {
            var acc = todayStat.Accounts.Find(c => (c.Number == AccountNumber));
            if (acc != null)
            {
                acc.Balance = Balance;
                acc.Equity = Equity;
            }
        }

        public bool CheckTradeAllowed(SignalInfo signal)
        {
            var acc = todayStat.Accounts.Find(c => (c.Number == signal.ObjectId));
            if (acc != null)
            {
                decimal balance = new decimal(0);
                decimal.TryParse((string)signal.Data, out balance);
                if (balance > 0)
                    acc.Balance = balance;
                CheckRiskForAccount(ref acc);
                signal.Value = acc.StopTrading ? 1 : 0;
                signal.Data = acc.StopReason;
                if (acc.StopTrading && !string.IsNullOrEmpty(acc.StopReason))
                {
                    IWebLog log = Program.Container.Resolve<IWebLog>();
                    if (log != null)
                        log.Log(acc.StopReason);
                }
                return acc.StopTrading;
            }
            return true;
        }

        protected void CheckRiskForAccount(ref Account acc)
        {
            DayOfWeek dow = DateTime.Now.DayOfWeek;
            // || (dow == DayOfWeek.Wednesday)
            if ((dow == DayOfWeek.Sunday) || (dow == DayOfWeek.Saturday)) 
            {
                acc.StopReason = "Today is non trading day of the week: " + dow.ToString() + "!!! RELAX!";
                acc.StopTrading = true;
                return;
            }
            decimal allowedLoss = todayStat.RISK_PER_DAY * acc.Balance;
            decimal startBalance = acc.Balance; //allowedLoss +
            if (startBalance <= 0)
            {
                //acc.StopReason = string.Format("Account [{0}] has zero balance!", acc.Number);
                //acc.StopTrading = true;
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

        public void DeletePosition(long Ticket)
        {
            lock (lockObject)
            {
                PositionInfo todel = null;
                positions.TryRemove(Ticket, out todel);
                RemovePosition(Ticket);
            }
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
            if (positions.ContainsKey(pos.Ticket))
            {
                positions[pos.Ticket].Role = pos.Role;
            }

        }

        public void RemovePosition(long Ticket)
        {
            service.SendMessage(WsMessageType.RemovePosition, Ticket);
        }
        #endregion


    }
}
