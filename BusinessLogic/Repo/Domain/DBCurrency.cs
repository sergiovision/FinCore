using System;
using System.Text;
using System.Collections.Generic;


namespace BusinessLogic.Repo
{
    public class DBCurrency : BaseEntity<DBCurrency>
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual int? Enabled { get; set; }
    }
}