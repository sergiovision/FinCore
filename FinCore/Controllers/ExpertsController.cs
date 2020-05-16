using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using System.Net.Http;
using System.Net;
using BusinessObjects.BusinessObjects;
using BusinessLogic.Repo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace FinCore.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("/api/[controller]")]
    public class ExpertsController : BaseController
    {
        [AllowAnonymous]
        [HttpGet]
        [AcceptVerbs("GET")]
        [Route("[action]")]
        public IActionResult GenerateDeployScripts()
        {
            try
            {
                string sourceFolder = MainService.GetGlobalProp(xtradeConstants.SETTINGS_PROPERTY_MQLSOURCEFOLDER);
                MainService.DeployToTerminals(sourceFolder);
                return Ok("Deploy Scripts OK");
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
            return Problem("Failed to deploy");
        }

        [HttpGet]
        [AcceptVerbs("GET")]
        [Route("[action]/{Id}")]
        public IActionResult DeployScript([FromRoute]int Id)
        {
            try
            {
                return Ok(MainService.DeployToAccount(Id));
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                return Problem($"Deploy to Account: {Id} FAILED: {e.ToString()}");
            }
        }

        [AcceptVerbs("PUT")]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("[action]")]
        public ActionResult UpdateAdviserState(Adviser adviser)
        {
            try
            {
                if (adviser == null)
                    return Problem("Empty Adviser passed to UpdateAdviserState method!", "Error", StatusCodes.Status417ExpectationFailed);

                if (MainService.UpdateObject(EntitiesEnum.Adviser, adviser.Id, JsonConvert.SerializeObject(adviser)) > 0)
                    return Ok();
                return Problem("Failed to update", "Error", StatusCodes.Status417ExpectationFailed);
            }
            catch (Exception e)
            {
                log.Info(e.ToString());
                return Problem(e.ToString(), "Error", StatusCodes.Status500InternalServerError);
            }
        }
    }

}
