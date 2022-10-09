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
using CryptoExchange.Net.CommonObjects;
using FTX.Net.Clients;
using FTX.Net.Enums;
using FTX.Net.Objects;
using FTX.Net.Objects.Models;
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
                RequestTimeout = TimeSpan.FromSeconds(60),
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

            var userTradesResult = await _kucoinClient.FuturesApi.Account.GetPositionsAsync();
            if (userTradesResult.Success)
            { 
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
    
    #region FTX
    
    private FTXClient _ftxClient;
    private bool initFtx()
    {
        if (_ftxClient == null)
        { 
            var conf = XTradeConfig.Self();
            _ftxClient = new FTXClient(new FTXClientOptions()
            {
                ApiCredentials = new ApiCredentials(conf.FTXAPIKey, conf.FTXAPISecret),
                LogLevel = LogLevel.Trace,
                RequestTimeout = TimeSpan.FromSeconds(60)
            });
        }
        return _ftxClient != null;
    }
    
    [HttpGet]
    [Route("[action]")]
    [AcceptVerbs("GET")]
    public async Task<ActionResult> FTXBalances()
    {
        try
        {
            if (!initFtx())
                return Ok("Failed to init FtxClient!");

            List<Account> result = new List<Account>();

            var accountData = await _ftxClient.TradeApi.Account.GetAccountInfoAsync();
            if (accountData.Success)
            {
                FTXAccountInfo fi = accountData.Data;
                return Ok(fi);
            }
            else
            {
                return Problem(accountData.Error?.ToString(), "Error", StatusCodes.Status400BadRequest);
            }
            
        }
        catch (Exception e)
        {
            return Problem(e.ToString(), "Error", StatusCodes.Status500InternalServerError);
        }
    }
    
    [HttpGet]
    [Route("[action]")]
    [AcceptVerbs("GET")]
    public ActionResult FTXTrades()
    {
        string msg = "";
        try
        {
            var te = MainService.Container.ResolveNamed<ITerminalEvents>("crypto");
            if (te == null)
                return null;
            var result =  te.GetAllPositions();
            return Ok(result);
        }
        catch (Exception e)
        {
            msg = e.ToString();
            log?.Error(msg);
        }
        return Problem(msg, "Error", StatusCodes.Status400BadRequest);
    }
    
    [HttpGet]
    [Route("[action]")]
    [AcceptVerbs("GET")]
    public async Task<ActionResult> FTXTradesRaw()
    {
        try
        {
            if (!initFtx())
                return Ok("Failed to init FtxClient!");

            var accountData = await _ftxClient.TradeApi.CommonFuturesClient.GetPositionsAsync();
            if (accountData.Success)
            {
                return Ok(accountData.Data);
            }
            return Problem(accountData.Error?.ToString(), "Error", StatusCodes.Status400BadRequest);
        }
        catch (Exception e)
        {
            return Problem(e.ToString(), "Error", StatusCodes.Status500InternalServerError);
        }
    }
    
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


    
    #endregion

}
