using FlowWing.API.Helpers;
using FlowWing.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using FlowWing.Business.Abstract;
using FlowWing.Entities;
using System.Text;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;

namespace FlowWing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAll")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly AppSettings _appSettings;
        public AuthController(IUserService userService, IRoleService roleService, IOptions<AppSettings> appSettings)
        {
            _userService = userService;
            _roleService = roleService;
            _appSettings = appSettings.Value;
        }

        /// <summary>
        /// Kullanıcı kaydı işlemleri burada gerçekleştirilir
        /// </summary>
        /// <param name="sicil"></param>
        /// <returns></returns>
        [HttpPost("signup")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SignUp(string sicil)
        {
            User user = await _userService.GetUserByEmailAsync(sicil + "@beko.com");
            if (user == null)
            {
                User newUser = new User
                {
                    Email = sicil + "@beko.com",
                    Password = PasswordHasher.HashPassword(sicil),
                    Username = sicil,
                    RoleId = 1,
                    IsApplicationUser = false,
                    LastLoginDate = DateTime.UtcNow,
                    CreationDate = DateTime.UtcNow
                };

                User createdUser = await _userService.CreateUserAsync(newUser);
                UserResponseModel response = new UserResponseModel
                {
                    Message = "Kayit Basarili",
                    Email = createdUser.Email,
                    Username = createdUser.Username,
                };

                return Ok(response);

            }
            else
            {
                // Kullanıcı zaten kayıtlı
                return BadRequest("Kullanici zaten kayitli");
            }
        }

        /// <summary>
        /// Kullanıcı giriş işlemleri burada gerçekleştirilir
        /// </summary>
        /// <param name="sicil"></param>
        /// <returns></returns>
        [HttpPost("login/{sicil}")]
        public async Task<IActionResult> Login(string sicil)
        {
            // Kullanıcı giriş işlemleri burada gerçekleştirilir
            User user = await _userService.GetUserByEmailAsync(sicil + "@beko.com");
            if (user == null)
            {
                return NotFound();
            }
            else
            {
                var role= _roleService.GetRoleByUserEmail(user.Email).Result;
                string token = JwtHelper.GenerateJwtToken(user.Id, user.Email, _appSettings.SecretKey, 30,role.Name);
                UserResponseModel response = new UserResponseModel
                {
                    Message = "Giris Basarili",
                    Email = user.Email,
                    Username = user.Username,
                    Token = token
                };

                return Ok(response);
            }
        }

        ///<summary>
        ///Check if user has admin role
        ///</summary>
        ///<returns></returns>
        [HttpGet("checkadminrole")]
        public async Task<IActionResult> CheckAdminRole()
        {
            string? roleName = HttpContext.Items["RoleName"] as string;
            if (roleName != "Admin")
            {
                return Unauthorized();
            }
            else
            {
                return Ok();
            }
        }
    }
}
