using FluentNHibernate.Mapping;

namespace BusinessLogic.Repo
{
    public class DBSiteMap : ClassMap<DBSite>
    {
        public DBSiteMap()
        {
            Table("site");
            LazyLoad();
            Id(x => x.Id).GeneratedBy.Identity().Column("ID");
            Map(x => x.Name).Column("Name");
            Map(x => x.Description).Column("Description");
            Map(x => x.Url).Column("URL");
        }
    }
}