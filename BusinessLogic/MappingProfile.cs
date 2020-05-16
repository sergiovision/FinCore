using System;
using AutoMapper;
using BusinessLogic.BusinessObjects;
using BusinessLogic.Repo;
using BusinessObjects;

namespace BusinessLogic
{
    public class MappingProfile : AutoMapper.Profile
    {
        public MappingProfile()
        {
            var types = MainService.thisGlobal.GetTypes();
            foreach( var t in types)
            {
                switch(t.Key)
                {
                    case EntitiesEnum.Account:
                        CreateMap<DBAccount, Account>(MemberList.None)
                            .ForMember(dto => dto.CurrencyStr, conf => conf.MapFrom(ol => ol.Currency.Name))
                            .ForMember(dto => dto.CurrencyId, conf => conf.MapFrom(ol => ol.Currency.Id))
                            .ForMember(dto => dto.PersonId, conf => conf.MapFrom(ol => ol.Person.Id))
                            .ForMember(dto => dto.WalletId, conf => conf.MapFrom(ol => ol.Wallet.Id)).PreserveReferences()
                            .ReverseMap()
                             // .ForMember(x => x.Person, opt => opt.Ignore())
                                .ForMember(x => x.Terminal, opt => opt.Ignore());
                        break;
                    case EntitiesEnum.Adviser:
                        CreateMap<DBAdviser, Adviser>(MemberList.None)
                            .ForMember(dto => dto.Symbol, conf => conf.MapFrom(ol => ol.Symbol.Name))
                            .ForMember(dto => dto.SymbolId, conf => conf.MapFrom(ol => ol.Symbol.Id))
                            .ForMember(dto => dto.MetaSymbol, conf => conf.MapFrom(ol => ol.Symbol.Metasymbol.Name))
                            .ForMember(dto => dto.MetaSymbolId, conf => conf.MapFrom(ol => ol.Symbol.Metasymbol.Id))
                            .ForMember(dto => dto.AccountNumber, conf => conf.MapFrom(ol => ol.Terminal.Accountnumber))
                            .ForMember(dto => dto.Broker, conf => conf.MapFrom(ol => ol.Terminal.Broker))
                            .ForMember(dto => dto.FullPath, conf => conf.MapFrom(ol => ol.Terminal.Fullpath))
                            .ForMember(dto => dto.CodeBase, conf => conf.MapFrom(ol => ol.Terminal.Codebase))
                            .ForMember(dto => dto.TerminalId, conf => conf.MapFrom(ol => ol.Terminal.Id))
                            .PreserveReferences()
                            .ReverseMap().ForMember(x => x.Symbol, opt => opt.Ignore())
                                         .ForMember(x => x.Terminal, opt => opt.Ignore()); 
                        break;
                    case EntitiesEnum.Deals:
                        CreateMap<DBDeals, DealInfo>(MemberList.None)
                            .ForMember(dto => dto.Account, conf => conf.MapFrom(ol => ol.Terminal.Account.Id))
                            .ForMember(dto => dto.AccountName, conf => conf.MapFrom(ol => ol.Terminal.Broker))
                            .ForMember(dto => dto.Symbol, conf => conf.MapFrom(ol => ol.Symbol.Name))
                            .ForMember(dto => dto.Ticket, conf => conf.MapFrom(ol => ol.Orderid))
                            .ForMember(dto => dto.Lots, conf => conf.MapFrom(ol => ol.Volume))
                            .ForMember(dto => dto.OpenTime, conf => conf.MapFrom(ol => ol.Opentime.ToString(xtradeConstants.MTDATETIMEFORMAT)))
                            .ForMember(dto => dto.CloseTime, conf => conf.MapFrom(ol => ol.Closetime.Value.ToString(xtradeConstants.MTDATETIMEFORMAT)))
                            .PreserveReferences()
                                .ReverseMap().ForMember(x => x.Symbol, opt => opt.Ignore());
                        break;
                    case EntitiesEnum.Rates:
                        CreateMap<DBRates, Rates>(MemberList.None)
                            .ForMember(dto => dto.MetaSymbol, conf => conf.MapFrom(ol => ol.Metasymbol.Name))
                            .ForMember(dto => dto.C1, conf => conf.MapFrom(ol => ol.Metasymbol.C1))
                            .ForMember(dto => dto.C2, conf => conf.MapFrom(ol => ol.Metasymbol.C2))
                            .PreserveReferences()
                                .ReverseMap().ForMember(x => x.Metasymbol, opt => opt.Ignore());
                        break;
                    case EntitiesEnum.Wallet:
                        CreateMap<DBWallet, Wallet>(MemberList.None)
                            .ReverseMap().ForMember(x => x.Person, opt => opt.Ignore())
                                         .ForMember(x => x.Site, opt => opt.Ignore());
                        break;
                    case EntitiesEnum.Terminal:
                        CreateMap<DBTerminal, Terminal>(MemberList.None)
                             .ForMember(dto => dto.Currency, conf => conf.MapFrom(ol => ol.Account.Currency.Name));
                        break;
                    case EntitiesEnum.Jobs:
                        CreateMap<DBJobs, ScheduledJobView>(MemberList.None)
                             .ForMember(dto => dto.Group, conf => conf.MapFrom(ol => ol.Grp))
                             .ForMember(dto => dto.Schedule, conf => conf.MapFrom(ol => ol.Cron))
                             .ForMember(dto => dto.IsRunning, opt => opt.Ignore())
                             .ForMember(dto => dto.Log, conf => conf.MapFrom(ol => ol.Statmessage));
                        break;
                    case EntitiesEnum.Person:
                        CreateMap<DBPerson, Person>(MemberList.None)
                             .ForMember(dto => dto.CountryId, opt => opt.Ignore())
                             .ForMember(dto => dto.Languageid, opt => opt.Ignore())
                             // .ForMember(dto => dto.Credential, opt => opt.Ignore())
                             .PreserveReferences()
                                .ReverseMap().ForMember(x => x.Country, opt => opt.Ignore());
                        break;
                    default:
                        CreateMap(t.Value.Item1, t.Value.Item2).ReverseMap().PreserveReferences();
                        break;
                }
            }

        }
    }
}
