using System;

namespace BusinessObjects.BusinessObjects
{
    public class PositionInfo
    {
        private static readonly Random random = new Random();
        public string AccountName { get; set; }
        public long Account { get; set; }
        public int Type { get; set; }
        public long Magic { get; set; }
        public long Ticket { get; set; }
        public double Lots { get; set; }
        public string Symbol { get; set; }
        public string MetaSymbol { get; set; }
        public double ProfitStopsPercent { get; set; }
        public double ProfitBricks { get; set; }
        public double Profit { get; set; }
        public string Role { get; set; }
        public double Openprice { get; set; }
        public double contractSize { get; set; }
        public string cur { get; set; }
        public double Value { get; set; }
        
        public double Vsl { get; set; }
        public double Realsl { get; set; }
        public double Vtp { get; set; }
        public double Realtp { get; set; }
        public double Swap { get; set; }
        public double Commission { get; set; }
        public string Expiration { get; set; } 
        
        public void Update()
        {
            var change = GenerateChange();
            var newProfit = change;
            Profit = newProfit;
        }

        public double calculateValue()
        {
            return Lots * contractSize * Openprice;
        }

        private double GenerateChange()
        {
            return (double) random.Next(-200, 200) / 10000;
        }
    }
}