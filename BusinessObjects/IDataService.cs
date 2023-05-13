using System;
using System.Collections.Generic;
using BusinessObjects.BusinessObjects;

namespace BusinessObjects;

public interface IDataService
{
    object GetObjects(EntitiesEnum type, bool showRetired);
    object GetChildObjects(EntitiesEnum parentType, EntitiesEnum childType, int parentKey, bool showRetired);
    object GetObject(EntitiesEnum type, int id);
    int InsertObject(EntitiesEnum type, string values);
    int UpdateObject(EntitiesEnum type, int id, string values);
    int DeleteObject(EntitiesEnum type, int id);

    List<CurrencyInfo> GetCurrencies();
    List<Wallet> GetWalletsState(DateTime forDate, bool showRetired);
    string GetGlobalProp(string name);
    void SetGlobalProp(string name, string value);
    Person LoginPerson(string mail, string password);
    void UpdateBalance(int TerminalId, decimal Balance, decimal Equity);
    void SaveDeals(List<DealInfo> deals);
    List<DealInfo> TodayDeals();
}
