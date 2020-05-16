using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace FinCore.Controllers
{
    public class BaseController : ControllerBase
    {
        protected static readonly ILog log = LogManager.GetLogger(typeof(BaseController));

        protected IMainService MainService;

        public BaseController()
        {
            MainService = Program.Container.Resolve<IMainService>();
        }

        #region GRUD
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("[action]/{type}")]
        public object GetObjects([FromRoute]int type)
        {
            try
            {
                EntitiesEnum t = (EntitiesEnum)type;
                object result = MainService.GetObjects(t);
                if (result != null)
                    return Ok(result);
                return Problem(String.Format("Failed to GetObjects{0}: result: {1}", t.ToString(), result), "Error", StatusCodes.Status417ExpectationFailed);
            }
            catch (Exception e)
            {
                log.Info(e.ToString());
                return Problem(e.ToString(), "Error", StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("[action]/{parentType}/{childType}/{parentKey}")]
        public object GetChildObjects([FromRoute]int parentType, [FromRoute]int childType, [FromRoute]int parentKey)
        {
            try
            {
                EntitiesEnum pT = (EntitiesEnum)parentType;
                EntitiesEnum cT = (EntitiesEnum)childType;
                object result = MainService.GetChildObjects(pT, cT, parentKey);
                if (result != null)
                    return Ok(result);
                return Problem(String.Format("Failed to GetChildObjects({0},{1}): result: {2}", pT.ToString(), cT.ToString(), result), "Error", StatusCodes.Status417ExpectationFailed);
            }
            catch (Exception e)
            {
                log.Info(e.ToString());
                return Problem(e.ToString(), "Error", StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("[action]/{type}")]
        public object GetObject([FromRoute]int type, [FromQuery] int id)
        {
            try
            {
                EntitiesEnum t = (EntitiesEnum)type;
                object result = MainService.GetObject(t, id);
                if (result != null)
                    return Ok(result);
                return Problem(String.Format("Failed to GetObject ({0},{1}): result: {2}", t.ToString(), id, result), "Error", StatusCodes.Status417ExpectationFailed);
            }
            catch (Exception e)
            {
                log.Info(e.ToString());
                return Problem(e.ToString(), "Error", StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("[action]/{type}")]
        public IActionResult InsertObject([FromRoute]int type, [FromBody]object value)
        {
            try
            {
                EntitiesEnum t = (EntitiesEnum)type;
                if (value == null) // (String.IsNullOrEmpty(values))
                    return Problem("Empty object passed to InsertObject method!", "Error", StatusCodes.Status417ExpectationFailed);
                int result = MainService.InsertObject((EntitiesEnum)type, value.ToString());
                if (result > 0)
                    return Ok(result);
                return Problem(String.Format("Failed to InsertObject {0}: result: {1}", t.ToString(), result), "Error", StatusCodes.Status417ExpectationFailed);

            }
            catch (Exception e)
            {
                log.Info(e.ToString());
                return Problem(e.ToString(), "Error", StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("[action]/{type}/{keyId}")]
        public IActionResult UpdateObject([FromRoute]int type, [FromRoute]int keyId, [FromBody]object value)
        {
            try
            {
                EntitiesEnum t = (EntitiesEnum)type;
                if (value == null) // (String.IsNullOrEmpty(values))
                    return Problem("Empty object passed to UpdateObject method!", "Error", StatusCodes.Status417ExpectationFailed);
                int result = MainService.UpdateObject((EntitiesEnum)type, keyId, value.ToString());
                if (result > 0)
                    return Ok(result);
                return Problem(String.Format("Failed to UpdateObject ({0},{1}): result: {2}", t.ToString(), keyId, result), "Error", StatusCodes.Status417ExpectationFailed);
            }
            catch (Exception e)
            {
                log.Info(e.ToString());
                return Problem(e.ToString(), "Error", StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete]
        [AcceptVerbs("DELETE")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("[action]/{type}")]
        public IActionResult DeleteObject([FromRoute]int type, [FromQuery]int key)
        {
            try
            {
                EntitiesEnum t = (EntitiesEnum)type;
                if (key <= 0)
                    return Problem("Invalid Id passed!", "Error", StatusCodes.Status417ExpectationFailed);
                int result = MainService.DeleteObject((EntitiesEnum)type, key);
                if (result > 0)
                    return Ok(result);
                return Problem(String.Format("Failed to DeleteObject {0}: result: {1}", t.ToString(), result), "Error", StatusCodes.Status417ExpectationFailed);
            }
            catch (Exception e)
            {
                log.Info(e.ToString());
                return Problem(e.ToString(), "Error", StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("[action]/{name}")]
        public object GetGlobalProp([FromRoute]string name)
        {
            try
            {
                object result = MainService.GetGlobalProp(name);
                if (result != null)
                    return Ok(result);
                return Problem(String.Format("Failed to GetGlobalProp ({0},{1}):", name, result), "Error", StatusCodes.Status417ExpectationFailed);
            }
            catch (Exception e)
            {
                log.Info(e.ToString());
                return Problem(e.ToString(), "Error", StatusCodes.Status500InternalServerError);
            }
        }
        #endregion



    }
}