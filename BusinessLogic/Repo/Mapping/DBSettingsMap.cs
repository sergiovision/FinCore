using BusinessLogic.Repo.Domain;
using FluentNHibernate.Mapping;

namespace BusinessLogic.Repo.Mapping
{
    public class DBSettingsMap : ClassMap<DBSettings>
    {
        public DBSettingsMap()
        {
            Table("settings");
            LazyLoad();
            Id(x => x.Id).GeneratedBy.Identity().Column("ID");
            Map(x => x.Propertyname).Column("Propertyname").Not.Nullable();
            Map(x => x.Value).Column("Value");
            Map(x => x.Description).Column("Description");
        }
    }
}