using BusinessObjects;
using BusinessObjects.BusinessObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinCore.Controllers;


// Angular example of login
// https://github.com/tjoudeh/AngularJSAuthentication
//http://bitoftech.net/2014/06/01/token-based-authentication-asp-net-web-api-2-owin-asp-net-identity/
[AllowAnonymous]
[ApiController]
[Route(xtradeConstants.API_ROUTE_CONTROLLER)]
public class PersonController : BaseController
{
    [HttpPost]
    [Route("[action]")]
    public ActionResult Register(Person userModel)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // TODO: Implement register user
        //IdentityResult result = await _repo.RegisterUser(userModel);

        //IHttpActionResult errorResult = GetErrorResult(result);

        //if (errorResult != null)
        //{
        //    return errorResult;
        //}

        return Ok();
    }
}
