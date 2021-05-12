using System;

namespace BusinessLogic.Repo.Domain
{
    public class DBJobs : BaseEntity<DBJobs>
    {
        public virtual int Id { get; set; }
        public virtual string Classpath { get; set; }
        public virtual string Grp { get; set; }
        public virtual string Name { get; set; }
        public virtual string Cron { get; set; }
        public virtual string Description { get; set; }
        public virtual string Statmessage { get; set; }
        public virtual DateTime Prevdate { get; set; }
        public virtual DateTime Nextdate { get; set; }
        public virtual string Params { get; set; }
        public virtual bool Disabled { get; set; }
    }
}