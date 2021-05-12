using System;

namespace BusinessObjects.BusinessObjects
{
    public class Rates : Idable
    {
        public string MetaSymbol { get; set; }
        public string C1 { get; set; }
        public string C2 { get; set; }
        public virtual decimal Ratebid { get; set; }
        public virtual decimal Rateask { get; set; }
        public virtual DateTime Lastupdate { get; set; }
        public virtual bool Retired { get; set; }
        public virtual int Id { get; set; }
    }

    public class RatesInfo
    {
        public virtual string Symbol { get; set; }
        public virtual double Ask { get; set; }
        public virtual double Bid { get; set; }
    }
}