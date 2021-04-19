using FluentNHibernate.Mapping;

namespace BusinessLogic.Repo
{
    public class DBSymbolMap : ClassMap<DBSymbol>
    {
        public DBSymbolMap()
        {
            Table("symbol");
            LazyLoad();
            Id(x => x.Id).GeneratedBy.Identity().Column("Id");
            References(x => x.Metasymbol).Column("MetaSymbolId");
            Map(x => x.Name).Column("Name").Not.Nullable();
            Map(x => x.Description).Column("Description");
            Map(x => x.Retired).Column("Retired");
            Map(x => x.Expiration).Column("Expiration");
        }
    }
}