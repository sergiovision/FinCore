using System;

namespace BusinessObjects.BusinessObjects
{
    public class AccountState : Idable
    {
        public int AccountId { get; set; }
        public DateTime Date { get; set; }
        public decimal Balance { get; set; }
        public string Comment { get; set; }
        public int Id { get; set; }
        
        public bool Retired { get; set; }
    }
}