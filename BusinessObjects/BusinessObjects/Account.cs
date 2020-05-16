using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public enum AccountType
    {
        Checking = 0,
        Investment = 1
    }

    public interface Idable
    {
        public int Id { get; set; }
    }

    public class Account : Idable
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public decimal Balance { get; set; }
        public decimal Equity { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyStr { get; set; }
        public int WalletId { get; set; }
        public int TerminalId { get; set; }
        public int PersonId { get; set; }
        public long Number { get; set; }
        public DateTime? Lastupdate { get; set; }
        public virtual bool Retired { get; set; }
        public AccountType Typ { get; set; }
        public decimal DailyProfit { get; set; }
        public decimal DailyMaxGain { get; set; }
        public bool StopTrading { get; set; }
        public string StopReason { get; set; }
    }
}
