using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class Terminal: Idable
    {
        public int Id { get; set; }
        public long AccountNumber { get; set; }

        public string Broker { get; set; }

        public string FullPath { get; set; }
        public string CodeBase { get; set; }
        public bool Retired { get; set; }
        public bool Demo { get; set; }
        public bool Stopped { get; set; }
        public string Currency { get; set; }
    }
}