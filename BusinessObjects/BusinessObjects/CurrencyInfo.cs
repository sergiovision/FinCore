using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace BusinessObjects
{
    public class CurrencyInfo
    {
        public short Id { get; set; }
        public string Name { get; set; }
        public bool Retired { get; set; }
    }
}