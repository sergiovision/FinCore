namespace BusinessLogic.Repo.Domain
{
    public class DBSite : BaseEntity<DBSite>
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual string Url { get; set; }
    }
}