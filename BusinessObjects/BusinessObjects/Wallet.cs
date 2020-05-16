using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class Wallet : Idable
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public int SiteId { get; set; }
        public string Name { get; set; }
        public string Shortname { get; set; }
        public bool Retired { get; set; }
        public decimal Balance { get; set; }
        public DateTime Date { get; set; }
        public List<Account> Accounts { get; set; }
    }
}