using System;

namespace BusinessLogic.Repo.Domain
{
    public class DBRates : BaseEntity<DBRates>
    {
        public virtual int Id { get; set; }
        public virtual DBMetasymbol Metasymbol { get; set; }
        public virtual decimal Ratebid { get; set; }
        public virtual decimal Rateask { get; set; }
        public virtual DateTime? Lastupdate { get; set; }
        public virtual bool Retired { get; set; }
    }
}