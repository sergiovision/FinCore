using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects;
using BusinessObjects.BusinessObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinCore.Controllers;

[AllowAnonymous]
[ApiController]
[Route(xtradeConstants.API_ROUTE_CONTROLLER)]
public class MTController : BaseController
{
    [HttpGet]
    [Route("[action]")]
    [AcceptVerbs("GET")]
    public ActionResult SendSignal()
    {
        Task<string> response = null;
        try
        {
            using (var reader = new StreamReader(Request.Body))
            {
                response = reader.ReadToEndAsync();
                if (string.IsNullOrEmpty(response.Result))
                    return Problem("Empty data passed as a parameter", "Error",
                        StatusCodes.Status500InternalServerError);
                var signal = SignalInfo.Create(response.Result);
                var result = MainService.SendSignal(signal);
                return Ok(result);
            }
        }
        catch (Exception e)
        {
            if (response != null && !string.IsNullOrEmpty(response.Result))
                log.Error($"Result error:{response.Result} {e}");
            else 
                log.Error(e.ToString());
            return Problem(e.ToString(), "Error", StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost]
    [AcceptVerbs("POST")]
    [Route("[action]")]
    public ActionResult PostSignal()
    {
        Task<string> response = null;
        SignalInfo signal = null;
        try
        {
            using (var reader = new StreamReader(Request.Body))
            {
                response = reader.ReadToEndAsync();
                if (string.IsNullOrEmpty(response.Result))
                    return Problem("NULL Signal passed as a parameter", "Error",
                        StatusCodes.Status500InternalServerError);
                signal = SignalInfo.Create(response.Result);

                if (signal == null)
                    return Problem("Broken Signal passed as a parameter", "Error",
                        StatusCodes.Status500InternalServerError);
                MainService.PostSignalTo(signal);
            }

            return Ok();
        }
        catch (Exception e)
        {
            if (response != null && !string.IsNullOrEmpty(response.Result))
            {
                log.Error($"Result error:{response.Result} {e}");
                if ( signal!= null)
                    log.Error($"Signal.Data={signal.Data}");
            } else 
                log.Error(e.ToString());
            return Problem(e.ToString(), "Error", StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet]
    [AcceptVerbs("GET")]
    [Route("[action]")]
    public ActionResult ListenSignal()
    {
        Task<string> response = null;
        try
        {
            using (var reader = new StreamReader(Request.Body))
            {
                response = reader.ReadToEndAsync();
                if (string.IsNullOrEmpty(response.Result))
                    return Ok();
                var signal = SignalInfo.Create(response.Result);
                var result = MainService.ListenSignal(signal.ObjectId, signal.Flags);
                return Ok(result);
            }
        }
        catch (Exception e)
        {
            if (response != null && !string.IsNullOrEmpty(response.Result))
                log.Error($"Result error:{response.Result} {e}");
            else 
                log.Error(e.ToString());
            return Problem(e.ToString(), "Error", StatusCodes.Status500InternalServerError);
        }
    }
}
