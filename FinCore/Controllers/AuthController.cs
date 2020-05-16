using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Autofac;
using BusinessObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FinCore.Controllers
{
    [ApiController]
    [Route("/api")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly JwtSettings _jwtSettings;
        protected IMainService MainService;


        public AuthController(JwtSettings jwtSettings)
        {
            _jwtSettings = jwtSettings;
            MainService = Program.Container.Resolve<IMainService>();
        }

        /* 
        [HttpPost]
        public async Task<IActionResult> CreateUser(LoginInfo loginInfo)
        {
            var result = await _userManager.CreateAsync(new TodoUser { UserName = loginInfo.UserName }, loginInfo.Password);

            if (result.Succeeded)
            {
                return Accepted();
            }

            return BadRequest(result.Errors);
        } 
        */


        [HttpPost("token")]
        public IActionResult GenerateToken(LoginInfo loginInfo)
        {
            Person result = MainService.LoginPerson(loginInfo.UserName, loginInfo.Password);
            if (result == null)
            {
                return Unauthorized("Username or password is wrong!");
            }

            var claims = new List<Claim>();

            claims.Add(new Claim("can_delete", "true"));
            claims.Add(new Claim("can_view", "true"));
            claims.Add(new Claim("sub", loginInfo.UserName));

            var key = new SymmetricSecurityKey(_jwtSettings.Key);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                    issuer: _jwtSettings.Issuer,
                    audience: _jwtSettings.Audience,
                    claims: claims,
                    expires: DateTime.Now.AddHours(xtradeConstants.TOKEN_LIFETIME_HOURS),
                    signingCredentials: creds
                );

            var user = new UserToken();
            user.access_token = new JwtSecurityTokenHandler().WriteToken(token);
            user.token_type = "Bearer";
            user.expires_in = (int)TimeSpan.FromHours(xtradeConstants.TOKEN_LIFETIME_HOURS).TotalSeconds;
            user.userName = loginInfo.UserName;
            return Ok(user);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("[action]/{symbol}")]
        public IActionResult levels([FromRoute]string symbol)
        {
            try
            {
                // TODO: implement getting levels
                return Ok("Not implemented");
                // return Problem(String.Format("Failed to get levels "), "Error", StatusCodes.Status417ExpectationFailed);
            }
            catch (Exception e)
            {
                return Problem(e.ToString(), "Error", StatusCodes.Status500InternalServerError);
            }
        }

    }
}
