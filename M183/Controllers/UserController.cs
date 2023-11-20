using M183.Controllers.Dto;
using M183.Controllers.Helper;
using M183.Data;
using M183.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace M183.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly NewsAppContext _context;
        private readonly IUserService _userService;

        public UserController(NewsAppContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        /// <summary>
        /// update password
        /// </summary>
        /// <response code="200">Password updated successfully</response>
        /// <response code="400">Bad request</response>
        /// <response code="404">User not found</response>
        [HttpPatch("password-update"), Authorize(Roles = "Admin,User")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public ActionResult PasswordUpdate(PasswordUpdateDto request)
        {
            if (request == null)
            {
                return BadRequest();
            }

            var user = _context.Users.Find(_userService.GetUserId());
            if (user == null)
            {
                return NotFound(string.Format("User {0} not found", _userService.GetUserId()));
            }
            user.IsAdmin = _userService.IsAdmin();
            user.Password = MD5Helper.ComputeMD5Hash(request.NewPassword);

            _context.Users.Update(user);
            _context.SaveChanges();

            return Ok();
        }
    }
}
