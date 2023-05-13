using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic.BusinessObjects;
using BusinessLogic.Repo.Domain;
using BusinessObjects;
using BusinessObjects.BusinessObjects;

namespace BusinessLogic.Repo;

public class WalletsRepository : BaseRepository<DBAccount>
{
    private readonly RatesService rates;

    public WalletsRepository(RatesService r)
    {
        rates = r;
    }

    public Wallet toDTO(DBWallet w)
    {
        var result = new Wallet();
        result.Id = w.Id;
        result.Name = w.Name;
        if (w.Person != null)
            result.PersonId = w.Person.Id;
        result.Retired = w.Retired;
        result.Shortname = w.Shortname;
        if (w.Site != null)
            result.SiteId = w.Site.Id;
        result.Accounts = new List<Account>();
        return result;
    }

    public List<Wallet> GetWallets()
    {
        var results = new List<Wallet>();
        using (var Session = ConnectionHelper.CreateNewSession())
        {
            var wallets = Session.Query<DBWallet>();
            foreach (var dbw in wallets) results.Add(toDTO(dbw));
        }

        return results;
    }

    public List<Wallet> GetWalletsState(DateTime forDate, bool showRetired)
    {
        var results = new List<Wallet>();
        using (var Session = ConnectionHelper.CreateNewSession())
        {
            // var rateList = Session.Query<DBRates>().Where(x => x.Retired == false).ToList();
            IQueryable<DBWallet> wallets = null;
            if (forDate == DateTime.MaxValue)
            {
                if (showRetired)
                    wallets = Session.Query<DBWallet>()
                        .Where(x => !x.Name.Equals("test"));
                else
                    wallets = Session.Query<DBWallet>()
                        .Where(x => x.Retired == false && !x.Name.Equals("test"));
            }
            else
            {
                if (showRetired)
                    wallets = Session.Query<DBWallet>().Where(x => !x.Name.Equals("test"));
                else
                    wallets = Session.Query<DBWallet>().Where(x => x.Retired == false && !x.Name.Equals("test"));
            }

            foreach (var dbw in wallets)
            {
                var wallet = toDTO(dbw);
                decimal balance = 0;
                IQueryable<DBAccount> accounts = null;
                if (forDate == DateTime.MaxValue)
                {
                    if (showRetired)
                        accounts = Session.Query<DBAccount>()
                            .Where(x => x.Wallet.Id == wallet.Id );
                    else 
                        accounts = Session.Query<DBAccount>()
                            .Where(x => x.Wallet.Id == wallet.Id && x.Retired == false);
                }
                else
                {
                    if (showRetired)
                        accounts = Session.Query<DBAccount>().Where(x => x.Wallet.Id == wallet.Id);
                    else 
                        accounts = Session.Query<DBAccount>().Where(x => x.Wallet.Id == wallet.Id && x.Retired == false);
                }

                foreach (var acc in accounts)
                {
                    var account = new Account();
                    if (DataService.toDTO(acc, ref account))
                    {
                        DBAccountstate accState = null;
                        IQueryable<DBAccountstate> accResults = null;
                        if (forDate.Equals(DateTime.MaxValue))
                            accResults = Session.Query<DBAccountstate>()
                                .Where(x => x.Account.Id == acc.Id)
                                .OrderByDescending(x => x.Date);
                        else
                            accResults = Session.Query<DBAccountstate>()
                                .Where(x => x.Account.Id == acc.Id && x.Date <= forDate)
                                .OrderByDescending(x => x.Date);

                        if (accResults == null || accResults.Count() == 0)
                            continue;
                        // acc.Currency.Id
                        accState = accResults.FirstOrDefault();
                        if (accState != null)
                        {
                            account.Balance = accState.Balance;
                            var value = account.Balance;
                            if (acc.Currency != null)
                                value = rates.ConvertToUSD(account.Balance, acc.Currency.Name);
                            balance += value;
                        }

                        wallet.Accounts.Add(account);
                    }
                }

                wallet.Balance = balance;
                results.Add(wallet);
            }
        }

        return results;
    }

    public List<Asset> AssetsDistribution(int type)
    {
        var results = new List<Wallet>();
        decimal totalBalance = 0;
        var dicAssets = new Dictionary<string, Asset>();
        using (var Session = ConnectionHelper.CreateNewSession())
        {
            var wallets = Session.Query<DBWallet>().Where(x => x.Retired == false && !x.Name.Equals("test"));
            foreach (var dbw in wallets)
            {
                var accounts = Session.Query<DBAccount>().Where(x => x.Wallet.Id == dbw.Id && x.Retired == false);
                foreach (var acc in accounts)
                {
                    if (acc.Currency == null)
                        continue;

                    var currency = acc.Currency.Name;
                    var accResults = Session.Query<DBAccountstate>()
                        .Where(x => x.Account.Id == acc.Id)
                        .OrderByDescending(x => x.Date);

                    if (accResults == null || accResults.Count() == 0)
                        continue;

                    var accState = accResults.FirstOrDefault();
                    if (accState != null)
                    {
                        var usdBalance = rates.ConvertToUSD(accState.Balance, acc.Currency.Name);
                        totalBalance += usdBalance;
                        Asset asset = null;
                        if (dicAssets.ContainsKey(currency))
                        {
                            asset = dicAssets[currency];
                        }
                        else
                        {
                            asset = new Asset();
                            asset.Id = acc.Currency.Id;
                            asset.Name = currency;
                            dicAssets.Add(currency, asset);
                        }

                        asset.Value += usdBalance;
                    }
                }
            }

            if (totalBalance > 0)
                foreach (var asset in dicAssets)
                    asset.Value.SharePercentValue = new decimal(100.0) * (asset.Value.Value / totalBalance);
        }

        return dicAssets.Values.ToList();
    }
}
