using FluentNHibernate.Mapping;

namespace BusinessLogic.Repo
{
    public class DBMetasymbolMap : ClassMap<DBMetasymbol>
    {
        public DBMetasymbolMap()
        {
            Table("metasymbol");
            LazyLoad();
            Id(x => x.Id).GeneratedBy.Identity().Column("Id");
            Map(x => x.Name).Column("Name").Not.Nullable();
            Map(x => x.Description).Column("Description");
            Map(x => x.C1).Column("C1");
            Map(x => x.C2).Column("C2");
            Map(x => x.Typ).Column("Typ");
            Map(x => x.Retired).Column("Retired");
        }
    }
}