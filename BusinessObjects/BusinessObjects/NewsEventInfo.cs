using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace BusinessObjects
{
    public class NewsEventInfo
    {
        public string Currency { get; set; }

        public string Name { get; set; }

        public sbyte Importance { get; set; }

        public string RaiseDateTime { get; set; }

        public string ForecastVal { get; set; }

        public string PreviousVal { get; set; }
    }
}