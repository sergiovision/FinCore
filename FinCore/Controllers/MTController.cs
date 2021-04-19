using BusinessObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.IO;

namespace FinCore.Controllers
{
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
            try
            {
                using (var reader = new StreamReader(Request.Body))
                {
                    var response = reader.ReadToEndAsync();
                    if (string.IsNullOrEmpty(response.Result))
                        return Problem("Empty data passed as a parameter", "Error", StatusCodes.Status500InternalServerError);
                    SignalInfo signal = JsonConvert.DeserializeObject<SignalInfo>(response.Result);
                    var result = MainService.SendSignal(signal);
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                return Problem(e.ToString(), "Error", StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        [AcceptVerbs("POST")]
        [Route("[action]")]
        public ActionResult PostSignal()
        {
            try
            {
                using (var reader = new StreamReader(Request.Body))
                {
                    var response = reader.ReadToEndAsync();
                    if (string.IsNullOrEmpty(response.Result))
                        return Problem("NULL Signal passed as a parameter", "Error", StatusCodes.Status500InternalServerError);
                    SignalInfo signal = JsonConvert.DeserializeObject<SignalInfo>(response.Result);

                    if (signal == null)
                        return Problem("Broken Signal passed as a parameter", "Error", StatusCodes.Status500InternalServerError);
                    MainService.PostSignalTo(signal);
                }
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
        [Route("[action]")]
        public ActionResult ListenSignal()
        {
            try
            {
                using (var reader = new StreamReader(Request.Body))
                {
                    var response = reader.ReadToEndAsync();
                    if (string.IsNullOrEmpty(response.Result))
                        return Ok();
                    SignalInfo signal = JsonConvert.DeserializeObject<SignalInfo>(response.Result);
                    if (signal == null)
                        return Problem("NULL Signal passed as a parameter", "Error", StatusCodes.Status500InternalServerError);
                    var result = MainService.ListenSignal(signal.ObjectId, signal.Flags);
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                return Problem(e.ToString(), "Error", StatusCodes.Status500InternalServerError);
            }
        }
    }
}