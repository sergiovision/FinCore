using Autofac;
using BusinessLogic.Repo;
using BusinessObjects;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace FinCore.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route(xtradeConstants.API_ROUTE_CONTROLLER)]
    public class PropsController : BaseController
    {
        [HttpGet]
        [AcceptVerbs("GET")]
        public IEnumerable<DynamicProperties> Get()
        {
            try
            {
                var ds = MainService.Container.Resolve<DataService>();
                if (ds == null)
                    return null;
                return ds.GetAllProperties();
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
        public DynamicProperties GetInstance([FromQuery] short entityType, [FromQuery] int objId)
        {
            try
            {
                var ds = MainService.Container.Resolve<DataService>();
                if (ds == null)
                    return null;
                return ds.GetPropertiesInstance(entityType, objId);
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
            return null;
        }

        [AcceptVerbs("PUT")]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status417ExpectationFailed)]
        [Route("[action]")]

        public ActionResult SaveInstance(DynamicProperties props)
        {
            try
            {
                if (props == null)
                    return Problem("Empty DynamicProperties passed to Put method!",
                        "Error", StatusCodes.Status500InternalServerError);

                var ds = MainService.Container.Resolve<DataService>();
                if (ds == null)
                    return null;
                if (ds.SavePropertiesInstance(props))
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

