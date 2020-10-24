using System;


namespace BusinessLogic.Repo
{
    public class DBProperties : BaseEntity<DBProperties>
    {
        public virtual int ID { get; set; }
        public virtual int objId { get; set; }
        public virtual short entityType { get; set; }
        public virtual string Vals { get; set; }
        public virtual DateTime? updated { get; set; }
    }
}