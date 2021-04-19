using System;

namespace BusinessObjects
{
    public class AccountState : Idable
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public DateTime Date { get; set; }
        public decimal Balance { get; set; }
        public string Comment { get; set; }
    }
}