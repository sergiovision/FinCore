using BusinessLogic.Repo.Domain;
using FluentNHibernate.Mapping;

namespace BusinessLogic.Repo.Mapping
{
    public class DBCountryMap : ClassMap<DBCountry>
    {
        public DBCountryMap()
        {
            Table("country");
            LazyLoad();
            Id(x => x.Id).GeneratedBy.Identity().Column("Id");
            References(x => x.Currency).Column("CurrencyId");
            Map(x => x.Code).Column("Code").Not.Nullable();
            Map(x => x.Description).Column("Description");
        }
    }
}