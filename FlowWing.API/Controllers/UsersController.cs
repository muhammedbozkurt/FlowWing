using FlowWing.Business.Abstract;
using FlowWing.Business.Concrete;
using FlowWing.DataAccess.Abstract;
using FlowWing.API.Helpers;
using FlowWing.Entities;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using FlowWing.API.Models;

namespace FlowWing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAll")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;
        private IEmailLogService _emailLogService;
        private AppSettings _appSettings;

        public UsersController(IUserService userService, IEmailLogService emailLogService, IOptions<AppSettings> appSettings)
        {
            _userService = userService;
            _emailLogService = emailLogService;
            _appSettings = appSettings.Value;
        }

        
        /// <summary>
        /// Get All Users
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
        

        /// <summary>
        /// Get User By Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }
        
        /// <summary>
        /// Authorize olmus user'in user loglarini getiren method
        /// </summary>
        /// <returns></returns>
        [HttpGet("logs")]
        public async Task<IActionResult> GetUserLogs()
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (JwtHelper.TokenIsValid(token,secretKey:_appSettings.SecretKey))
            {
                (string UserEmail, string UserId, object roleName) = JwtHelper.GetJwtPayloadInfo(token);
                var user = await _userService.GetUserByEmailAsync(UserEmail);
                if (user == null)
                {
                    return NotFound();
                }
                var logs = await _emailLogService.GetEmailLogsByUserIdAsync(int.Parse(UserId));
                return Ok(logs);
            }
            else
            {
                return Unauthorized();
            }
        }
        /// <summary>
        /// Create an User
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateModel userModel)
        {
            bool isAplicationUser = false;
            if  (userModel.isApplicationUser == 1)
            {
                isAplicationUser = true;
            }

            int roleId = 1;
            if (userModel.RoleName == "Admin")
            {
                roleId = 2;
            }
            User user = new User
            {
                Email = userModel.Sicil + "@beko.com",
                Username = userModel.Sicil,
                Password = PasswordHasher.HashPassword(userModel.Sicil),
                RoleId = roleId,
                IsApplicationUser = isAplicationUser,
                LastLoginDate = DateTime.UtcNow,
                CreationDate = DateTime.UtcNow
            };

            User createdUser = await _userService.CreateUserAsync(user);
            UserResponseModel response = new UserResponseModel
            {
                Message = "Kayit Basarili",
                Email = createdUser.Email
            };

            return Ok(response);
        }
        /// <summary>
        /// Delete an user
        /// </summary>
        /// <param name="sicil"></param>
        /// <returns></returns>
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string sicil)
        {
            User user = await _userService.GetUserByEmailAsync(sicil + "@beko.com");
            if (user == null)
            {
                return NotFound();
            }
            await _userService.DeleteUserAsync(user);
            return Ok();
        }

        /// <summary>
        /// Update an User
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] User user)
        {
            var existingUser = await _userService.GetUserByIdAsync(user.Id);
            if (existingUser == null)
            {
                return NotFound();
            }

            var updatedUser = await _userService.UpdateUserAsync(user);
            return Ok(updatedUser);
        }
    }
}
