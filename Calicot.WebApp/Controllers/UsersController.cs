#nullable disable
using Microsoft.AspNetCore.Mvc;
using Calicot.Shared.Models;
using Calicot.Shared.Services;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Calicot.Shared.Helpers;

namespace Calicot.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        public class AuthenticateRequest
        {
            [Required]
            public string IdToken { get; set; } = default!;
        }
        private IUserService _userService;

        private readonly JwtGenerator _jwtGenerator;

        private readonly string _GoogleClientId = default!;
        private readonly string _GoogleClientSecret = default!;

        public UsersController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _GoogleClientId = configuration.GetSection("Authentication:Google:ClientId").Value;
            _GoogleClientSecret = configuration.GetSection("Authentication:Google:ClientSecret").Value;
            var jwtPrivateKey = configuration.GetValue<string>("JwtPrivateSigningKey");
            _jwtGenerator = new JwtGenerator(jwtPrivateKey);
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] AuthenticateRequest data)
        {
            GoogleJsonWebSignature.ValidationSettings settings = new GoogleJsonWebSignature.ValidationSettings();

            // Change this to your google client ID
            settings.Audience = new List<string>() { _GoogleClientId };
            
            GoogleJsonWebSignature.Payload payload  = GoogleJsonWebSignature.ValidateAsync(data.IdToken, settings).Result;

            var user = _userService.GetByEmail(payload.Email);
            if(user != null) 
            {
                return Ok(new { AuthToken = _jwtGenerator.CreateUserAuthToken(payload.Email) });
            }

            return BadRequest(new { message = "Gmail user not authorized." });
            
        }

        [AllowAnonymous]
        [HttpPost("authenticateOld")]
        public IActionResult AuthenticateOld(AuthenticateRequestUser model)
        {
            var response = _userService.Authenticate(model);

            if (response == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(response);
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            return Ok(users);
        }
    }
}