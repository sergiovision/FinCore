using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Repo.Domain;
using BusinessObjects;
using BusinessObjects.BusinessObjects;
using Newtonsoft.Json;

namespace BusinessLogic.BusinessObjects;

public class RatesService
{
    private ConcurrentDictionary<string, Rates> crates;
    private static IWebLog log;

    public RatesService(IWebLog l)
    {
        log = l;
    }

    public bool Init()
    {
        crates = new ConcurrentDictionary<string, Rates>();
        GetRates(true);
        return true;
    }
    
    public ConcurrentDictionary<string, Rates> GetRates(bool isReread)
    {
        try
        {
            if (Utils.HasAny(crates) && !isReread)
                return crates;
            using (var Session = ConnectionHelper.CreateNewSession())
            {
                var dbrates = Session.Query<DBRates>();
                foreach (var dbr in dbrates)
                {
                    var r = toDTO(dbr);
                    crates[r.MetaSymbol] = r;
                }
            }
        }
        catch (Exception e)
        {
            log.Error("Error: GetRates: " + e);
        }
        return crates;
    }

    public Rates toDTO(DBRates rates)
    {
        var result = new Rates();
        result.MetaSymbol = rates.Metasymbol.Name;
        result.Symbol = rates.Symbol;
        result.C1 = rates.Metasymbol.C1;
        result.C2 = rates.Metasymbol.C2;
        result.Ratebid = rates.Ratebid;
        result.Rateask = rates.Rateask;
        result.Retired = rates.Retired;
        if (rates.Lastupdate.HasValue)
            result.Lastupdate = rates.Lastupdate.Value;
        else
            result.Lastupdate = DateTime.UtcNow;
        return result;
    }
    
    public decimal ConvertToUSD(decimal value, string valueCurrency)
    {
        var result = value;
        if (valueCurrency.Equals("USD") || value == 0)
            return result;
        var c1sym = valueCurrency + "USD";
        if (crates.ContainsKey(c1sym))
        {
            var finalRate = crates[c1sym];
            result = result * finalRate.Ratebid;
        }
        else
        {
            var c2sym = "USD" + valueCurrency;
            if (crates.ContainsKey(c2sym))
            {
                var finalRate = crates[c2sym];
                result = result / finalRate.Rateask;
            }
        }

        return result;
    }

    public string GetRatesList()
    {
        var stringBuilder = new StringBuilder();
        var list = GetRates(true);
        foreach (var rate in list)
        {
            if (!string.IsNullOrEmpty(rate.Value.Symbol))
            {
                stringBuilder.Append(rate.Value.Symbol);
                stringBuilder.Append(',');
            }
        }
        return stringBuilder.ToString();
    } 
    
    public void UpdateRates(List<RatesInfo> rInfo)
    {
        if (!Utils.HasAny(rInfo))
        {
            GetRates(true);
            return;
        }
        try
        {
            using (var Session = ConnectionHelper.CreateNewSession())
            {
                var dbrates = Session.Query<DBRates>();
                foreach (var rate in dbrates)
                {
                    var symbolName = rate.Symbol;  // rate.Metasymbol.Name; change to active chart symbol
                    var currentRate = rInfo.FirstOrDefault(d => d.Symbol.CompareTo(symbolName) == 0);
                    if (currentRate == null)
                        continue;
                    using (var Transaction = Session.BeginTransaction())
                    {
                        rate.Rateask = new decimal(currentRate.Ask);
                        rate.Ratebid = new decimal(currentRate.Bid);
                        rate.Lastupdate = DateTime.UtcNow;
                        Session.Update(rate);
                        Transaction.Commit();
                    }
                }
            }

            GetRates(true);
        }
        catch (Exception e)
        {
            log.Error("Error: UpdateRates: " + e);
        }
    }
    
    
    public RatesInfo GetBYNRates()
    {
        try
        {
            const string url = "https://www.nbrb.by/api/exrates/rates?periodicity=0";
            var client = new HttpClient();
            var stringTask = client.GetStringAsync(url);
            double result = 0;
            var stringData = stringTask.GetAwaiter().GetResult();
            {
                
                var data = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(stringData);
                if (data != null)
                    foreach (var item in data)
                        if (item.ContainsKey("Cur_ID"))
                        {
                            var value = (long) item["Cur_ID"];
                            if (value == 145) 
                                result = (double) item["Cur_OfficialRate"];
                        }
            }
            if (result > 0)
            {
                var rate = new RatesInfo();
                rate.Ask = result;
                rate.Bid = result;
                rate.Symbol = "USDBYN";
                return rate;
            }
        }
        catch (Exception e)
        {
            return null;
        }
        return null;
    }

}