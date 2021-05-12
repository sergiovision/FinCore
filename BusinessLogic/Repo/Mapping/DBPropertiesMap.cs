using BusinessLogic.Repo.Domain;
using FluentNHibernate.Mapping;

namespace BusinessLogic.Repo.Mapping
{
    public class DBPropertiesMap : ClassMap<DBProperties>
    {
        public DBPropertiesMap()
        {
            Table("properties");
            LazyLoad();
            Id(x => x.ID).GeneratedBy.Identity().Column("ID");
            Map(x => x.objId).Column("objId").Not.Nullable();
            Map(x => x.entityType).Column("entityType").Not.Nullable();
            Map(x => x.Vals).Column("Vals");
            Map(x => x.updated).Column("updated");
        }
    }
}