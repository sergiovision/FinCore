using System;
using System.Collections.Generic;

namespace BusinessObjects
{
    public interface IDataService
    {
        object GetObjects(EntitiesEnum type);
        object GetChildObjects(EntitiesEnum parentType, EntitiesEnum childType, int parentKey);
        object GetObject(EntitiesEnum type, int id);
        int InsertObject(EntitiesEnum type, string values);
        int UpdateObject(EntitiesEnum type, int id, string values);
        int DeleteObject(EntitiesEnum type, int id);

        List<CurrencyInfo> GetCurrencies();
        List<Wallet> GetWalletsState(DateTime forDate);
        string GetGlobalProp(string name);
        void SetGlobalProp(string name, string value);
        Person LoginPerson(string mail, string password);
        void UpdateBalance(int TerminalId, decimal Balance, decimal Equity);
        void SaveDeals(List<DealInfo> deals);
        decimal ConvertToUSD(decimal value, string valueCurrency);
        List<DealInfo> TodayDeals();
    }
}