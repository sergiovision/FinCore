using FluentNHibernate.Mapping;

namespace BusinessLogic.Repo
{
    public class DBRatesMap : ClassMap<DBRates>
    {
        public DBRatesMap()
        {
            Table("rates");
            LazyLoad();
            Id(x => x.Id).GeneratedBy.Identity().Column("Id");
            References(x => x.Metasymbol).Column("MetaSymbolId");
            Map(x => x.Ratebid).Column("Ratebid").Not.Nullable();
            Map(x => x.Rateask).Column("Rateask");
            Map(x => x.Lastupdate).Column("Lastupdate");
            Map(x => x.Retired).Column("Retired");
        }
    }
}