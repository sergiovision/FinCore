using System;
using System.Collections.Generic;

namespace BusinessObjects.BusinessObjects
{
    public class Wallet : Idable
    {
        public int PersonId { get; set; }
        public int SiteId { get; set; }
        public string Name { get; set; }
        public string Shortname { get; set; }
        public bool Retired { get; set; }
        public decimal Balance { get; set; }
        public DateTime Date { get; set; }
        public List<Account> Accounts { get; set; }
        public int Id { get; set; }
    }
}