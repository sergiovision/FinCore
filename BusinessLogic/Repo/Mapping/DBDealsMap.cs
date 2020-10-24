using FluentNHibernate.Mapping;

namespace BusinessLogic.Repo
{
    public class DBDealsMap : ClassMap<DBDeals>
    {
        public DBDealsMap()
        {
            Table("deals");
            LazyLoad();
            Id(x => x.Id).GeneratedBy.Identity().Column("Id");
            References(x => x.Terminal).Column("TerminalId");
            References(x => x.Symbol).Column("SymbolId");
            References(x => x.Adviser).Column("AdviserId");
            Map(x => x.Orderid).Column("OrderId");
            Map(x => x.Dealid).Column("DealId");
            Map(x => x.Opentime).Column("Opentime").Not.Nullable();
            Map(x => x.Typ).Column("Typ").Not.Nullable();
            Map(x => x.Volume).Column("Volume");
            Map(x => x.Price).Column("Price");
            Map(x => x.Closetime).Column("Closetime");
            Map(x => x.Comment).Column("comment");
            Map(x => x.Commission).Column("Commission");
            Map(x => x.Swap).Column("Swap");
            Map(x => x.Profit).Column("Profit");
        }
    }
}