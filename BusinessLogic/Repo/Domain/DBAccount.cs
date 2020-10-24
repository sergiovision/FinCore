using System;


namespace BusinessLogic.Repo
{
    public class DBAccount : BaseEntity<DBAccount>
    {
        public virtual int Id { get; set; }
        public virtual DBCurrency Currency { get; set; }
        public virtual DBWallet Wallet { get; set; }
        public virtual DBTerminal Terminal { get; set; }
        public virtual DBPerson Person { get; set; }
        public virtual int Number { get; set; }
        public virtual string Description { get; set; }
        public virtual decimal Balance { get; set; }
        public virtual decimal Equity { get; set; }
        public virtual DateTime? Lastupdate { get; set; }
        public virtual bool Retired { get; set; }
        public virtual int Typ { get; set; }
    }
}