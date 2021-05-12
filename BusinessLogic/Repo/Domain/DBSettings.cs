namespace BusinessLogic.Repo.Domain
{
    public class DBSettings : BaseEntity<DBSettings>
    {
        public virtual int Id { get; set; }
        public virtual string Propertyname { get; set; }
        public virtual string Value { get; set; }
        public virtual string Description { get; set; }
    }
}