using Microsoft.AspNetCore.Mvc;
using SIFAIBackend.Business;
using SIFAIBackend.Entities;
using System;
using System.Threading.Tasks;

namespace SIFAIBackend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            try
            {
                var token = await _authService.AuthenticateAsync(model.Email, model.Password);
                if (!string.IsNullOrEmpty(token))
                {
                    // Başarılı giriş
                    return Ok(new { Token = token, Message = "Başarılı giriş" });
                }
                else
                {
                    // Kullanıcı doğrulanamadı
                    return BadRequest(new { message = "Geçersiz e-posta veya şifre" });
                }
            }
            catch (Exception ex)
            {
                // Hata meydana geldi
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
