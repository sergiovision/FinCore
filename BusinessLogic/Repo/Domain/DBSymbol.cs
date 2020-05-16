using System;
using System.Text;
using System.Collections.Generic;


namespace BusinessLogic.Repo
{
    public class DBSymbol : BaseEntity<DBSymbol>
    {
        public virtual int Id { get; set; }
        public virtual DBMetasymbol Metasymbol { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual int? Retired { get; set; }
        public virtual DateTime? Expiration { get; set; }
    }
}