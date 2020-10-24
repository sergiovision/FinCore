using FluentNHibernate.Mapping;

namespace BusinessLogic.Repo
{
    public class DBJobsMap : ClassMap<DBJobs>
    {
        public DBJobsMap()
        {
            Table("jobs");
            LazyLoad();
            Id(x => x.Id).GeneratedBy.Identity().Column("Id");
            Map(x => x.Classpath).Column("Classpath").Not.Nullable();
            Map(x => x.Grp).Column("Grp").Not.Nullable();
            Map(x => x.Name).Column("Name").Not.Nullable();
            Map(x => x.Cron).Column("Cron").Not.Nullable();
            Map(x => x.Description).Column("Description");
            Map(x => x.Statmessage).Column("Statmessage");
            Map(x => x.Prevdate).Column("Prevdate").Not.Nullable();
            Map(x => x.Nextdate).Column("Nextdate").Not.Nullable();
            Map(x => x.Params).Column("Params");
            Map(x => x.Disabled).Column("Disabled").Not.Nullable();
        }
    }
}