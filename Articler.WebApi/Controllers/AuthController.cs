using Articler.AppDomain.Models.Auth;
using Articler.WebApi.Services.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Articler.WebApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;

        private readonly IConfiguration _configuration;
        private readonly IJwtSecurityService _jwtSecurityService;
        private readonly Services.Auth.IAuthenticationService _authorizationService;

        public AuthController(ILogger<AuthController> logger, 
            IConfiguration configuration, IJwtSecurityService jwtSecurityService,
            Services.Auth.IAuthenticationService authenticationService)
        {
            _logger = logger;

            _configuration = configuration;

            _jwtSecurityService = jwtSecurityService;
            _authorizationService = authenticationService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult SignInPmi()
        {
            _logger.LogInformation("AuthController::SignInPmi: Start PMI Club authentication");

            var schemeName = _configuration["Auth:Oidc:Name"];
            if (schemeName == null)
            {
                _logger.LogError("AuthController::SignInPmi: scheme name is null");
                return BadRequest("Oidc scheme name is null");
            }

            var props = new AuthenticationProperties
            {
                RedirectUri = new PathString("/auth/SinginCallback"),
                Items =
                {
                    { "scheme", schemeName }
                }
            };

            _logger.LogInformation("AuthController::SignInPmi: Start Oidc challenge with scheme name {schemeName}", schemeName);
            return Challenge(props, schemeName);
        }

        [AllowAnonymous]
        public async Task<IActionResult> SingInCallback()
        {
            _logger.LogInformation("AuthController::SinginCallback: Start authentication callback");

            // Read the outcome of external auth
            var tempAuthName = _configuration["Auth:TempCookieName"];
            if (tempAuthName == null)
            {
                _logger.LogError("AuthController::SinginCallback: AuthTempCookieName is null");
                return LocalRedirect(new PathString("/"));
            }

            _logger.LogInformation("AuthController::SinginCallback: begin read external authentication");
            var authResult = await HttpContext.AuthenticateAsync(tempAuthName);

            if (!authResult.Succeeded)
            {
                _logger.LogError("AuthController::SinginCallback: Can't read the outcome of external authentication");
                return LocalRedirect(new PathString("/"));
            }

            // Read metadata with scheme
            var metadata = authResult.Properties.Items;
            if ((metadata == null) || (!metadata.TryGetValue("scheme", out string? value)) || (string.IsNullOrEmpty(value)))
            {
                _logger.LogError("AuthController::SinginCallback: Metadata doesn't contain scheme");
                return LocalRedirect(new PathString("/"));
            }

            var schemeName = metadata["scheme"]!;
            _logger.LogInformation("AuthController::SinginCallback: authentication scheme name is \"{schemeName}\"", schemeName);

            try
            {
                var user = await _authorizationService.AuthenticateAsync(
                    schemeName: schemeName,
                    claims: authResult.Principal.Claims,
                    metadata: metadata!
                );

                _logger.LogInformation("AuthController::SinginCallback: Authenticated user {usrEmail} {usrFirstName}",
                    user.Email, user.FirstName);

                var accountType = user.AccountType switch
                {
                    AccountType.Free => "Free",
                    AccountType.Trial => "Trial",
                    AccountType.Paid => "Paid",
                    _ => throw new NotImplementedException(),
                };

                var claims = new List<Claim>
                {
                    new("email", user.Email),
                    new("AccountType", accountType),
                };

                // Create JWT token
                var jwtToken = _jwtSecurityService.CreateJwtToken(claims);

                HttpContext.Response
                    .Cookies
                    .Append(
                        key: _configuration["Auth:AuthDefaultSchemeName"]!,
                        value: jwtToken,
                        options: new CookieOptions()
                        {
                            MaxAge = TimeSpan.FromDays(1)
                        }
                    );

                // signout from temp cookie
                await HttpContext.SignOutAsync(tempAuthName);

                _logger.LogInformation("AuthController::SinginCallback: Success SignIn user");
                return LocalRedirect(new PathString("/projects"));
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "AuthController::SinginCallback: Can't authentificate user");
                return LocalRedirect(new PathString("/"));
            }
        }

        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("AuthController::Logout: start logout user");

            var userEmail = User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Email)
                ?.Value;
            var userName = User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                ?.Value;
            
            _logger.LogInformation("AuthController::Logout: sign out user {usrEmail} {usrName}",
                userEmail, userName);

            HttpContext.Response
                .Cookies
                .Delete(_configuration["Auth:AuthDefaultSchemeName"]!);

            _logger.LogInformation("AuthController::Logout: removed cookie for user with email={usrEmail}",
                userEmail);
            return await Task.FromResult(LocalRedirect(new PathString("/")));
        }
    }
}
