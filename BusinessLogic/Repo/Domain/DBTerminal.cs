namespace BusinessLogic.Repo
{
    public class DBTerminal : BaseEntity<DBTerminal>
    {
        public virtual int Id { get; set; }
        public virtual DBAccount Account { get; set; }
        public virtual int? Accountnumber { get; set; }
        public virtual string Broker { get; set; }
        public virtual string Fullpath { get; set; }
        public virtual string Codebase { get; set; }
        public virtual bool Retired { get; set; }
        public virtual bool Demo { get; set; }
        public virtual bool Stopped { get; set; }
    }
}