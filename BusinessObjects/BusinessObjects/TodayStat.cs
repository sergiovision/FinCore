using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BusinessObjects
{
    public class TodayStat
    {
        public TodayStat()
        {
            Deals = new List<DealInfo>();
            Accounts = new List<Account>();
        }

        public decimal TodayGainReal { get; set; }
        public decimal TodayGainDemo { get; set; }
        public decimal TodayGainRealPercent { get; set; }
        public decimal TodayGainDemoPercent { get; set; }
        public decimal TodayBalanceReal { get; set; }
        public decimal TodayBalanceDemo { get; set; }

        // risk properties
        // Max Amount in % you may loose daily
        public decimal RISK_PER_DAY { get; set; }
        // Min daily gain that taken into account to do checks losses after gains
        public decimal DAILY_MIN_GAIN { get; set; }
        // Losses in % after gains today
        public decimal DAILY_LOSS_AFTER_GAIN { get; set; }
        // end of risk properties
        public List<DealInfo> Deals { get; set; }
        public List<Account> Accounts { get; set; }
    }
}
