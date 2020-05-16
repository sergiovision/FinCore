using System;
using System.Text;
using System.Collections.Generic;


namespace BusinessLogic.Repo
{
    public class DBWallet : BaseEntity<DBWallet>
    {
        public virtual int Id { get; set; }
        public virtual DBPerson Person { get; set; }
        public virtual DBSite Site { get; set; }
        public virtual string Name { get; set; }
        public virtual string Shortname { get; set; }
        public virtual string Link { get; set; }
        public virtual bool Retired { get; set; }
    }
}