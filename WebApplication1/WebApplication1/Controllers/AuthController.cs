using WebApplication1.models.auth_model;
using WebApplication1.Repository.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using WebApplication1.models.dto;
using WebApplication1.models;



namespace WebApplication1.Controllers

{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        public AuthController(IAuthService authService , UserManager<ApplicationUser> userManager)
        {
            _authService = authService;
            _userManager = userManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(model);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpGet("verify")]
        public async Task<IActionResult> VerifyEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return BadRequest();
            }
            await _userManager.ConfirmEmailAsync(user, token);
            // Verify the user by updating the verification status

            return Ok("Email verified successfully.");
        }

        [HttpPut("ForgetPassword", Name = "ForgetPassword")]
        public async Task<IActionResult> ForgetPassword(string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
                return BadRequest("Email Not Found");
            var result = await _authService.SendResetPasswordEmailAsync(user);
            return Ok(result);
        }

        [HttpPut("ResetPassword", Name = "ResetPassword")]
        public async Task<IActionResult> ResetPassword(ChangePasswordDto model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);

            if (user == null)
            {
                return BadRequest();
            }

            var result = await _userManager.ResetPasswordAsync(user, model.ChangePasswordTokken, model.NewPassword);

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> login([FromBody] TokenRequestModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.GetTokenAsync(model);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpPost("addrole")]
        public async Task<IActionResult> AddRoleAsync([FromBody] AddRoleModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.AddRoleAsync(model);

            if (!string.IsNullOrEmpty(result))
                return BadRequest(result);

            return Ok(model);
        }
    }


}