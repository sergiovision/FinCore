using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using BusinessLogic;
using BusinessObjects;
using BusinessObjects.BusinessObjects;
using CryptoExchange.Net.Authentication;
using Kucoin.Net.Clients;
using Kucoin.Net.Objects;
using Kucoin.Net.Objects.Models.Spot;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderStatus = Kucoin.Net.Enums.OrderStatus;

namespace FinCore.Controllers;

[AllowAnonymous]
[ApiController]
[Route(xtradeConstants.API_ROUTE_CONTROLLER)]
public class CryptoController : BaseController
{
    #region KuCoin

    private KucoinClient _kucoinClient;

    private bool initKuCoin()
    {
        if (_kucoinClient == null)
        {
            var conf = XTradeConfig.Self();
            _kucoinClient = new KucoinClient(new KucoinClientOptions()
            {
                ApiCredentials = new KucoinApiCredentials(conf.KuCoinAPIKey, conf.KuCoinAPISecret, conf.KuCoinPassPhrase),
                LogLevel = LogLevel.Trace,
                //RequestTimeout = TimeSpan.FromSeconds(60),
                FuturesApiOptions = new KucoinRestApiClientOptions
                {
                    ApiCredentials = new KucoinApiCredentials(conf.KuCoinFutureAPIKey, conf.KuCoinFutureAPISecret, conf.KuCoinFuturePassPhrase),
                    AutoTimestamp = false
                }
            });
        }
        return _kucoinClient != null;
    }
    
    [HttpGet]
    [Route("[action]")]
    [AcceptVerbs("GET")]
    public async Task<ActionResult> KuCoinBalances()
    {
        try
        {
            if (!initKuCoin())
                return Ok("Failed to init KuCoinClient!");

            List<Account> result = new List<Account>();
            decimal minimum = new decimal(0.001);
            var accountData = await _kucoinClient.SpotApi.Account.GetAccountsAsync();
            if (accountData.Success)
            {
                foreach (var account in accountData.Data)
                {
                    if (account.Total > minimum)
                    {
                        var acc = new Account();
                        acc.Equity = account.Available;
                        acc.Balance = account.Total;
                        acc.Description = account.Asset;
                        acc.CurrencyStr = account.Id;
                        result.Add(acc);
                    }
                }
            }
            return Ok(result);
        }
        catch (Exception e)
        {
            return Problem(e.ToString(), "Error", StatusCodes.Status500InternalServerError);
        }
    }
    
    [HttpGet]
    [Route("[action]")]
    [AcceptVerbs("GET")]
    public async Task<ActionResult> KuCoinTrades()
    {
        try
        {
            if (!initKuCoin())
                return Ok("Failed to init KuCoinClient!");

            List<PositionInfo> result = new List<PositionInfo>();

            //var userTradesResult = await _kucoinClient.FuturesApi.Trading.GetUserTradesAsync();
            var userTradesResult = await _kucoinClient.FuturesApi.Account.GetPositionsAsync();
            if (userTradesResult.Success)
            {
                /* foreach (var order in userTradesResult.Data.Items)
                {
                    var pos = new PositionInfo();
                    pos.Lots = Convert.ToDouble(order.QuoteQuantity);
                    pos.Symbol = order.Symbol;
                    pos.Type = (int)order.Type;
                    pos.Role = order.TradeType;
                    result.Add(pos);
                } */
                return Ok(userTradesResult.Data);
            }
            return Ok("Fail to get trades!");
        }
        catch (Exception e)
        {
            return Problem(e.ToString(), "Error", StatusCodes.Status500InternalServerError);
        }
        
    }
    
    [HttpGet]
    [Route("[action]")]
    [AcceptVerbs("GET")]
    public async Task<ActionResult> KuCoinOrders()
    {
        try
        {
            if (!initKuCoin())
                return Ok("Failed to init KuCoinClient!");

            List<PositionInfo> result = new List<PositionInfo>();

            var userTradesResult = await _kucoinClient.FuturesApi.Trading.GetOrdersAsync(null, OrderStatus.Active);
            if (userTradesResult.Success)
            {
                /* foreach (var order in userTradesResult.Data.Items)
                {
                    var pos = new PositionInfo();
                    pos.Lots = Convert.ToDouble(order.QuoteQuantity);
                    pos.Symbol = order.Symbol;
                    pos.Type = (int)order.Type;
                    pos.Role = order.TradeType.ToString();
                    result.Add(pos);
                } */
                return Ok(userTradesResult.Data.Items);
            }
            return Ok("Fail to get orders");
        }
        catch (Exception e)
        {
            return Problem(e.ToString(), "Error", StatusCodes.Status500InternalServerError);
        }
    }
    
    [HttpGet]
    [Route("[action]")]
    [AcceptVerbs("GET")]
    public async Task<ActionResult> KuCoinTriggerOrders()
    {
        try
        {
            if (!initKuCoin())
                return Ok("Failed to init KuCoinClient!");

            List<PositionInfo> result = new List<PositionInfo>();

            var userTradesResult = await _kucoinClient.FuturesApi.Trading.GetUntriggeredStopOrdersAsync();
            if (userTradesResult.Success)
            {
                /* foreach (var order in userTradesResult.Data.Items)
                {
                    var pos = new PositionInfo();
                    pos.Lots = Convert.ToDouble(order.QuoteQuantity);
                    pos.Symbol = order.Symbol;
                    pos.Type = (int)order.Type;
                    pos.Role = order.TradeType.ToString();
                    result.Add(pos);
                } */
                return Ok(userTradesResult.Data.Items);
            }
            return Ok("Fail to get orders");
        }
        catch (Exception e)
        {
            return Problem(e.ToString(), "Error", StatusCodes.Status500InternalServerError);
        }
    }


    #endregion
    
    [HttpGet]
    [AcceptVerbs("GET")]
    [Route("[action]")]
    public IEnumerable<DealInfo> GetToday()
    {
        try
        {
            var te = MainService.Container.ResolveNamed<ITerminalEvents>("crypto");
            if (te == null)
                return null;
            return te.GetTodayDeals();
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }

        return null;
    }

}
