using System;
using System.Text;
using System.Collections.Generic;


namespace BusinessLogic.Repo
{
    public class DBLaststate : BaseEntity<DBLaststate>
    {
        public virtual int Id { get; set; }
        public virtual DBWallet Wallet { get; set; }
        public virtual string Name { get; set; }
        public virtual decimal Balance { get; set; }
        public virtual DateTime Date { get; set; }
    }
}