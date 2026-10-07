using System.Security.Claims;
using DVLD.Api.Authentication;
using DVLD_Buisness;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DVLD.Api.Controllers
{
    [ApiController]
    [Route("api/mobile-auth")]
    public sealed class MobileAuthController : ControllerBase
    {
        private static readonly PasswordHasher<clsMobileUser>
            _passwordHasher = new();

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] MobileLoginRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrEmpty(request.Password))
            {
                return Unauthorized(new
                {
                    message = "Invalid username or password."
                });
            }

            string username =
                request.Username.Trim().ToLowerInvariant();

            clsMobileUser mobileUser =
                clsMobileUser.FindByUsername(username);

            if (mobileUser == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid username or password."
                });
            }

            PasswordVerificationResult verificationResult =
                _passwordHasher.VerifyHashedPassword(
                    mobileUser,
                    mobileUser.PasswordHash,
                    request.Password);

            if (verificationResult ==
                PasswordVerificationResult.Failed)
            {
                return Unauthorized(new
                {
                    message = "Invalid username or password."
                });
            }

            if (!mobileUser.IsActive)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message = "Account is inactive."
                    });
            }

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    mobileUser.MobileUserID.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    mobileUser.Username),

                new Claim(
                    MobileAuthDefaults.PersonIdClaim,
                    mobileUser.PersonID.ToString()),

                new Claim(
                    MobileAuthDefaults.AccountTypeClaim,
                    MobileAuthDefaults.MobileCitizenAccountType)
            };

            var identity = new ClaimsIdentity(
                claims,
                MobileAuthDefaults.Scheme);

            var principal =
                new ClaimsPrincipal(identity);

            var authenticationProperties =
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    AllowRefresh = false
                };

            await HttpContext.SignInAsync(
                MobileAuthDefaults.Scheme,
                principal,
                authenticationProperties);

            return Ok(new
            {
                personId = mobileUser.PersonID,
                fullName = mobileUser.PersonInfo?.FullName,
                username = mobileUser.Username
            });
        }
    }

    public sealed class MobileLoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}