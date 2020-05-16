using System;
using System.Text;
using System.Collections.Generic;


namespace BusinessLogic.Repo
{
    public class DBCountry : BaseEntity<DBCountry>
    {
        public virtual int Id { get; set; }
        public virtual DBCurrency Currency { get; set; }
        public virtual string Code { get; set; }
        public virtual string Description { get; set; }
    }
}