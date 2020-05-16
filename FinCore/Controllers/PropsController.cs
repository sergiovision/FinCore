using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using System.Net.Http;
using System.Net;
using BusinessLogic.Repo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;

namespace FinCore.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("/api/[controller]")]
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
        public DynamicProperties GetInstance([FromQuery]short entityType, [FromQuery]int objId)
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

