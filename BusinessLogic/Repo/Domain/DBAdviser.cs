namespace BusinessLogic.Repo.Domain
{
    public class DBAdviser : BaseEntity<DBAdviser>
    {
        public virtual int Id { get; set; }
        public virtual DBTerminal Terminal { get; set; }
        public virtual DBSymbol Symbol { get; set; }
        public virtual string Name { get; set; }
        public virtual string Timeframe { get; set; }
        public virtual bool IsMaster { get; set; }
        public virtual bool Running { get; set; }
        public virtual bool Retired { get; set; }
    }
}