using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace BusinessObjects
{
    public class DealInfo
    {
        public long Ticket { get; set; }

        public int Type { get; set; }
        public long Magic { get; set; }
        public string Symbol { get; set; }
        public double Lots { get; set; }
        public double OpenPrice { get; set; }
        public double ClosePrice { get; set; }
        public string OpenTime { get; set; }
        public string CloseTime { get; set; }
        public double Profit { get; set; }
        public double SwapValue { get; set; }
        public double Commission { get; set; }
        public long Account { get; set; }
        public string AccountName { get; set; }
        public string Comment { get; set; }
        public long OrderId { get; set; }
    }
}