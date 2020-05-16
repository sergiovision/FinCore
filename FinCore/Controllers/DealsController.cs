using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects;
using System;
using System.Collections.Generic;
using Autofac;
using System.Net;
using BusinessLogic.Repo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace FinCore.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("/api/[controller]")]
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
        public IEnumerable<MetaSymbolStat> MetaSymbolStatistics([FromQuery] int type)
        {
            try
            {
                var ds = MainService.Container.Resolve<DataService>();
                if (ds == null)
                    return null;
                return ds.MetaSymbolStatistics(type);
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
        public ActionResult ClosePosition([FromQuery] int account, [FromQuery] int Magic, [FromQuery] int Ticket)
        {
            try
            {
                SignalInfo signalPos = null;
                if (Ticket > 0)
                {
                    signalPos = MainService.CreateSignal(SignalFlags.Terminal, account, EnumSignals.SIGNAL_CLOSE_POSITION);
                } else
                {
                    signalPos = MainService.CreateSignal(SignalFlags.Expert, Magic, EnumSignals.SIGNAL_CLOSE_POSITION);
                }
                signalPos.Value = Ticket;
                signalPos.Data = Magic.ToString();
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
                List<object> mss = (List<object>)MainService.GetObjects(EntitiesEnum.MetaSymbol);
                foreach (var m in mss)
                {
                    MetaSymbol ms = (MetaSymbol)m;
                    SignalInfo signalC = MainService.CreateSignal(SignalFlags.Cluster, ms.Id, EnumSignals.SIGNAL_ACTIVE_ORDERS);
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
}