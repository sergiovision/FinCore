using System;
using Autofac;
using BusinessObjects;
using BusinessObjects.BusinessObjects;
using log4net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinCore.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class BaseController : ControllerBase
{
    protected static readonly ILog log = LogManager.GetLogger(typeof(BaseController));

    protected IMainService MainService;

    public BaseController()
    {
        MainService = Program.Container.Resolve<IMainService>();
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Route("[action]/{name}")]
    public object GetGlobalProp([FromRoute] string name)
    {
        try
        {
            object result = MainService.GetGlobalProp(name);
            if (result != null)
                return Ok(result);
            return Problem(string.Format("Failed to GetGlobalProp ({0},{1}):", name, result), "Error",
                StatusCodes.Status417ExpectationFailed);
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
    [Route("[action]")]
    public object LogList()
    {
        try
        {
            var result = MainService.LogList();
            if (result != null)
                return Ok(result);
            return Problem(string.Format("Failed to LogList {0}", result), "Error",
                StatusCodes.Status417ExpectationFailed);
        }
        catch (Exception e)
        {
            log.Info(e.ToString());
            return e.ToString();
        }
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Route("[action]")]
    public string GetLogContent([FromQuery] string logName, [FromQuery] int size)
    {
        try
        {
            var result = MainService.GetLogContent(logName, size);
            if (result != null)
                return result;
            return "no log file content";
        }
        catch (Exception e)
        {
            log.Info(e.ToString());
            return e.ToString();
        }
    }

    #region GRUD

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Route("[action]/{type}")]
    public object GetObjects([FromRoute] int type, [FromQuery]bool showRetired)
    {
        try
        {
            var t = (EntitiesEnum) type;
            var result = MainService.GetObjects(t, showRetired);
            if (result != null)
                return Ok(result);
            return Problem(string.Format("Failed to GetObjects{0}: result: {1}", t.ToString(), result), "Error",
                StatusCodes.Status417ExpectationFailed);
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
    public object GetChildObjects([FromRoute] int parentType, [FromRoute] int childType, [FromRoute] int parentKey, [FromQuery]bool showRetired)
    {
        try
        {
            var pT = (EntitiesEnum) parentType;
            var cT = (EntitiesEnum) childType;
            var result = MainService.GetChildObjects(pT, cT, parentKey, showRetired);
            if (result != null)
                return Ok(result);
            return Problem(
                string.Format("Failed to GetChildObjects({0},{1}): result: {2}", pT.ToString(), cT.ToString(),
                    result), "Error", StatusCodes.Status417ExpectationFailed);
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
    public object GetObject([FromRoute] int type, [FromQuery] int id)
    {
        try
        {
            var t = (EntitiesEnum) type;
            var result = MainService.GetObject(t, id);
            if (result != null)
                return Ok(result);
            return Problem(string.Format("Failed to GetObject ({0},{1}): result: {2}", t.ToString(), id, result),
                "Error", StatusCodes.Status417ExpectationFailed);
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
    public IActionResult InsertObject([FromRoute] int type, [FromBody] object value)
    {
        try
        {
            var t = (EntitiesEnum) type;
            if (value == null) // (String.IsNullOrEmpty(values))
                return Problem("Empty object passed to InsertObject method!", "Error",
                    StatusCodes.Status417ExpectationFailed);
            var result = MainService.InsertObject((EntitiesEnum) type, value.ToString());
            if (result > 0)
                return Ok(result);
            return Problem(string.Format("Failed to InsertObject {0}: result: {1}", t.ToString(), result), "Error",
                StatusCodes.Status417ExpectationFailed);
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
    public IActionResult UpdateObject([FromRoute] int type, [FromRoute] int keyId, [FromBody] object value)
    {
        try
        {
            var t = (EntitiesEnum) type;
            if (value == null) // (String.IsNullOrEmpty(values))
                return Problem("Empty object passed to UpdateObject method!", "Error",
                    StatusCodes.Status417ExpectationFailed);
            var result = MainService.UpdateObject((EntitiesEnum) type, keyId, value.ToString());
            if (result > 0)
                return Ok(result);
            return Problem(
                string.Format("Failed to UpdateObject ({0},{1}): result: {2}", t.ToString(), keyId, result),
                "Error", StatusCodes.Status417ExpectationFailed);
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
    public IActionResult DeleteObject([FromRoute] int type, [FromQuery] int key)
    {
        try
        {
            var t = (EntitiesEnum) type;
            if (key <= 0)
                return Problem("Invalid Id passed!", "Error", StatusCodes.Status417ExpectationFailed);
            var result = MainService.DeleteObject((EntitiesEnum) type, key);
            if (result > 0)
                return Ok(result);
            return Problem(string.Format("Failed to DeleteObject {0}: result: {1}", t.ToString(), result), "Error",
                StatusCodes.Status417ExpectationFailed);
        }
        catch (Exception e)
        {
            log.Info(e.ToString());
            return Problem(e.ToString(), "Error", StatusCodes.Status500InternalServerError);
        }
    }

    #endregion

    
}
