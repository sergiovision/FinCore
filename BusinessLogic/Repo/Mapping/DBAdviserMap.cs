using BusinessLogic.Repo.Domain;
using FluentNHibernate.Mapping;

namespace BusinessLogic.Repo.Mapping
{
    public class DBAdviserMap : ClassMap<DBAdviser>
    {
        public DBAdviserMap()
        {
            Table("adviser");
            LazyLoad();
            Id(x => x.Id).GeneratedBy.Identity().Column("Id");
            References(x => x.Terminal).Column("TerminalId");
            References(x => x.Symbol).Column("SymbolId");
            Map(x => x.Name).Column("Name");
            Map(x => x.Timeframe).Column("Timeframe");
            Map(x => x.IsMaster).Column("IsMaster");
            Map(x => x.Running).Column("Running");
            Map(x => x.Retired).Column("Retired");
        }
    }
}