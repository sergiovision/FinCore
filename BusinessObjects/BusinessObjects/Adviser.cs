using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class Adviser : Idable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public long AccountNumber { get; set; }
        public string Broker { get; set; }
        public string FullPath { get; set; }
        public string CodeBase { get; set; }
        public int TerminalId { get; set; }
        public int SymbolId { get; set; }
        public string Symbol { get; set; }
        public string MetaSymbol { get; set; }
        public int MetaSymbolId { get; set; }
        public string Timeframe { get; set; }
        public bool Running { get; set; }
        public bool Retired { get; set; }
        public bool IsMaster { get; set; }
    }
}