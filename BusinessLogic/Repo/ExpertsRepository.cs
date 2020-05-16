using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
//using BusinessLogic.BusinessObjects;
using BusinessObjects;
//using Newtonsoft.Json;
using NHibernate;

namespace BusinessLogic.Repo
{
    public class ExpertsRepository : BaseRepository<DBAdviser>
    {
        public List<Adviser> GetAdvisers()
        {
            List<Adviser> results = new List<Adviser>();
            using (ISession Session = ConnectionHelper.CreateNewSession())
            {
                var advisers = Session.Query<DBAdviser>();
                foreach (var dbadv in advisers)
                {
                    Adviser adv = new Adviser();
                    if (toDTO(dbadv, ref adv))
                        results.Add(adv);
                }
            }

            return results;
        }

        /*
        public void MigrateAdvisersData()
        {
            var ds = MainService.thisGlobal.Container.Resolve<DataService>();

            // List<Adviser> results = new List<Adviser>();
            using (ISession Session = ConnectionHelper.CreateNewSession())
            {
                var advisers = Session.Query<DBAdviser>();
                foreach (var dbadv in advisers)
                {
                    //Adviser adv = new Adviser();
                    //if (toDTO(dbadv, ref adv))
                    //    results.Add(adv);
                    try
                    {
                        if (String.IsNullOrEmpty(dbadv.State))
                            continue;
                        Dictionary<string, object> res = JsonConvert.DeserializeObject<Dictionary<string, object>>(dbadv.State);

                        Dictionary<string, TDynamicProperty<object> > result = new Dictionary<string, TDynamicProperty<object> >();
                        foreach(var prop in res)
                        {
                            TDynamicProperty<object> p = new TDynamicProperty<object>();
                            if (prop.Value != null)
                            {
                                p.type = prop.Value.GetType().ToString();
                                switch (p.type)
                                {
                                    case "System.Int64":
                                        p.type = "integer";
                                        break;
                                    case "System.Double":
                                        p.type = "double";
                                        break;
                                    case "System.Boolean":
                                        p.type = "boolean";
                                        break;
                                    default:
                                        p.type = "string";
                                        break;
                                }

                            }
                            else p.type = "string";
                            if (prop.Key.Equals("InfoTextColor"))
                                p.type = "hexinteger";

                            p.value = prop.Value;
                            result.Add(prop.Key, p);
                        }

                        DynamicProperties dp = new DynamicProperties();
                        dp.entityType = (short)EntitiesEnum.Adviser;
                        dp.objId = dbadv.Id;
                        dp.updated = DateTime.UtcNow;
                        dp.Vals = JsonConvert.SerializeObject(result);
                        ds.SavePropertiesInstance(dp);

                    } catch (Exception e)
                    {
                        log.Info("Failed to migrate: Adviser: " + dbadv.Id.ToString() + ", " + e.ToString());
                    }
                }
            }
        }*/


        public static bool toDTO(DBAdviser adv, ref Adviser result)
        {
            try
            {
                result.Id = adv.Id;
                result.Name = adv.Name;
                result.Running = adv.Running;
                if (adv.Terminal != null)
                {
                    result.TerminalId = adv.Terminal.Id;
                    result.CodeBase = adv.Terminal.Codebase;
                    result.Broker = adv.Terminal.Broker;
                    result.FullPath = adv.Terminal.Fullpath;
                    if (adv.Terminal.Accountnumber != null)
                        result.AccountNumber = adv.Terminal.Accountnumber.Value;
                }

                result.Retired = adv.Retired;
                if (adv.Symbol != null)
                {
                    result.Symbol = adv.Symbol.Name;
                    result.SymbolId = adv.Symbol.Id;
                }

                result.MetaSymbolId = adv.Symbol.Metasymbol.Id;
                result.MetaSymbol = adv.Symbol.Metasymbol.Name;

                result.Timeframe = adv.Timeframe;
                // result.State = adv.State;
                return true;
            } catch
            {
                return false;
            }
        }
    }
}