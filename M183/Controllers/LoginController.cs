using M183.Controllers.Dto;
using M183.Controllers.Helper;
using M183.Data;
using M183.Models;
using M183.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace M183.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly NewsAppContext _context;

        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;

        public LoginController(NewsAppContext context, IConfiguration configuration, IUserService userService)
        {
            _context = context;
            _configuration = configuration;
            _userService = userService;
        }

        [HttpGet("GetUsername"), Authorize]
        public ActionResult<string> GetUsername()
        {
            var username = _userService.GetUsername();
            return Ok(username);
        }

        [HttpGet("GetRole"), Authorize]
        public ActionResult<bool> GetRole()
        {
            var role = _userService.IsAdmin();
            return Ok(role);
        }

        [HttpGet("GetId"), Authorize]
        public ActionResult<int> GetId()
        {
            var id = _userService.GetUserId();
            return Ok(id);
        }


        /// <summary>
        /// Login a user using password and username
        /// </summary>
        /// <response code="200">Login successfull</response>
        /// <response code="400">Bad request</response>
        /// <response code="401">Login failed</response>
        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public ActionResult<User> Login(LoginDto request)
        {
            if (request == null || request.Username.IsNullOrEmpty() || request.Password.IsNullOrEmpty())
            {
                return BadRequest();
            }
            string username = request.Username;
            string password = request.Password;

            User? user = _context.Users.Where(x => x.Username == username && x.Password == MD5Helper.ComputeMD5Hash(password)).FirstOrDefault();
            if (user == null)
            {
                return Unauthorized("login failed");
            }
            var token = CreateToken(user);
            return Ok(token);
        }

        private string CreateToken(User user)
        {
            if (user != null)
            {
                var issuer = _configuration.GetSection("Jwt:Issuer").Value;
                var audience = _configuration.GetSection("Jwt:Audience").Value;
                var key = Encoding.ASCII.GetBytes(_configuration.GetSection("Jwt:Key").Value);

                var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username.ToString())
        };

                // Fügen Sie die Rolle basierend auf der IsAdmin-Eigenschaft hinzu
                if (user.IsAdmin)
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, "User"));
                }

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(5),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwtToken = tokenHandler.WriteToken(token);

                return jwtToken;
            }

            return "Fehler";
        }
    }


}
