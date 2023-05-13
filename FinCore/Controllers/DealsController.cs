using System;
using System.Collections.Generic;
using System.Net;
using Autofac;
using BusinessLogic.Repo;
using BusinessObjects;
using BusinessObjects.BusinessObjects;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinCore.Controllers;
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route(xtradeConstants.API_ROUTE_CONTROLLER)]
public class DealsController : BaseController
{
    [HttpGet]
    [AcceptVerbs("GET")]
    public IEnumerable<DealInfo> Get()
    {
        try
        {
            return MainService.GetDeals();
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }

        return null;
    }

    [HttpGet]
    [AcceptVerbs("GET")]
    [Route("[action]")]
    public IEnumerable<DealInfo> GetToday()
    {
        try
        {
            var ds = MainService.Container.Resolve<ITerminalEvents>();
            if (ds == null)
                return null;
            return ds.GetTodayDeals();
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }

        return null;
    }

    [HttpGet]
    [AcceptVerbs("GET")]
    [Route("[action]")]
    public TodayStat GetTodayStat()
    {
        try
        {
            var ds = MainService.Container.Resolve<ITerminalEvents>();
            if (ds == null)
                return null;
            return ds.GetTodayStat();
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }

        return null;
    }


    [HttpGet]
    [AcceptVerbs("GET")]
    [Route("[action]")]
    public IEnumerable<MetaSymbolStat> MetaSymbolStatistics([FromQuery] int count, [FromQuery] int option)
    {
        try
        {
            var ds = MainService.Container.Resolve<DataService>();
            if (ds == null)
                return null;
            return ds.MetaSymbolStatistics(count, option);
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }
        return null;
    }

    [HttpGet]
    [AcceptVerbs("GET")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Route("[action]")]
    public ActionResult ClosePosition([FromQuery] int account, [FromQuery] int magic, [FromQuery] int Ticket)
    {
        try
        {
            SignalInfo signalPos = null;
            if (magic > 0)
            {
                signalPos = MainService.CreateSignal(SignalFlags.Expert, magic, EnumSignals.SIGNAL_CLOSE_POSITION,
                    0);
                signalPos.SetData(magic.ToString());
            }
            else
            {
                signalPos = MainService.CreateSignal(SignalFlags.Terminal, account, EnumSignals.SIGNAL_CLOSE_POSITION, 0);
                signalPos.SetData("0");
            }
            signalPos.Value = Ticket;
            MainService.PostSignalTo(signalPos);
            var terminals = MainService.Container.Resolve<ITerminalEvents>();
            if (terminals != null)
                terminals.DeletePosition(Ticket);
            return Ok();
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
            return Problem(e.ToString(), "Error", StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet]
    [AcceptVerbs("GET")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Route("[action]")]
    public ActionResult RefreshAll()
    {
        try
        {
            var mss = (List<object>) MainService.GetObjects(EntitiesEnum.MetaSymbol, false);
            foreach (var m in mss)
            {
                var ms = (MetaSymbol) m;
                if (ms.Retired)
                    continue;
                var signalC = MainService.CreateSignal(SignalFlags.Cluster, ms.Id, EnumSignals.SIGNAL_ACTIVE_ORDERS,
                    0);
                MainService.PostSignalTo(signalC);
            }

            return Ok(HttpStatusCode.OK);
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
            return Problem(e.ToString(), "Error", StatusCodes.Status500InternalServerError);
        }
    }
}
