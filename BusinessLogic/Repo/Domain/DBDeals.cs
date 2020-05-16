using System;
using System.Text;
using System.Collections.Generic;


namespace BusinessLogic.Repo
{
    public class DBDeals : BaseEntity<DBDeals>
    {
        public virtual int Id { get; set; }
        public virtual DBTerminal Terminal { get; set; }
        public virtual DBSymbol Symbol { get; set; }
        public virtual DBAdviser Adviser { get; set; }
        public virtual int? Orderid { get; set; }
        public virtual int? Dealid { get; set; }
        public virtual DateTime Opentime { get; set; }
        public virtual int Typ { get; set; }
        public virtual decimal Volume { get; set; }
        public virtual decimal Price { get; set; }
        public virtual DateTime? Closetime { get; set; }
        public virtual string Comment { get; set; }
        public virtual decimal Commission { get; set; }
        public virtual decimal Swap { get; set; }
        public virtual decimal Profit { get; set; }
    }
}