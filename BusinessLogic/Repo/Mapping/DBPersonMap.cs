using FluentNHibernate.Mapping;

namespace BusinessLogic.Repo
{
    public class DBPersonMap : ClassMap<DBPerson>
    {
        public DBPersonMap()
        {
            Table("person");
            LazyLoad();
            Id(x => x.Id).GeneratedBy.Identity().Column("Id");
            References(x => x.Country).Column("CountryId");
            Map(x => x.Created).Column("Created").Not.Nullable();
            Map(x => x.Languageid).Column("LanguageId").Not.Nullable();
            Map(x => x.Credential).Column("Credential").Not.Nullable();
            Map(x => x.Regip).Column("RegIp").Not.Nullable();
            Map(x => x.Mail).Column("Mail").Not.Nullable();
            Map(x => x.Privilege).Column("Privilege");
            Map(x => x.Uuid).Column("Uuid");
            Map(x => x.Activated).Column("Activated");
            Map(x => x.Retired).Column("Retired");
        }
    }
}