using System;

namespace BusinessObjects.BusinessObjects
{
    public class MetaSymbolStat
    {
        public long MetaId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int NumOfTrades { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal ProfitPerTrade { get; set; }
        public DateTime Date { get; set; }
    }
}