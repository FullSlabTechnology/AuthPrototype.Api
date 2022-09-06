using AuthPrototype.Api.Models;
using AuthPrototype.Api.Repository;
using AuthPrototype.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace AuthPrototype.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ITokenService _tokenService;
        private readonly IUserRepository _userRepository;

        public AuthController(ITokenService tokenService, IUserRepository userRepository, IConfiguration configuration)
        {
            _tokenService = tokenService;
            _userRepository = userRepository;
            _config = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Authenticate([FromBody] UserLogin user)
        {

            var validUser = _userRepository.GetUser(user);
            if (validUser is not null)
            {
                var token = _tokenService.BuildToken(_config["Jwt:Key"].ToString(), _config["Jwt:Issuer"].ToString(), validUser);
                HttpContext.Session.SetString("Token", token);
                // TODO: Might need to login when building the token, use StudentTeacherApi project
                return Ok(new
                {
                    UserName = user.UserName,
                    Token = token
                });
            }
            return Unauthorized();
        }

        [Authorize]
        [HttpGet("validate")]
        public async Task<IActionResult> ValidateToken()
        {
            var result = _tokenService.IsTokenValid(_config["Jwt:Key"].ToString(), _config["Jwt:Issuer"].ToString(), HttpContext.Session.GetString("Token"));
            return Ok(result);
        }

        [Authorize]
        [HttpGet("user")]
        public async Task<IActionResult> GetUser()
        {
            return Ok(HttpContext.User);
        }
    }
}
