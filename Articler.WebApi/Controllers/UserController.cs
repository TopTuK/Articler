using Articler.WebApi.Models;
using Articler.WebApi.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Articler.WebApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class UserController(
        ILogger<UserController> logger,
        IAuthenticationService authenticationService
        ) : ControllerBase
    {
        private readonly ILogger<UserController> _logger = logger;
        private readonly IAuthenticationService _authenticationService = authenticationService;

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUser()
        {
            var userId = HttpContext.Items["UserId"]!.ToString()!;
            _logger.LogInformation("UserController::GetUser: start get user information. " +
                "UserId={userId}", userId);

            try
            {
                var user = await _authenticationService.GetUserAsync(userId);
                if (user.AccountType == AppDomain.Models.Auth.AccountType.None)
                {
                    _logger.LogError("UserController::GetUser: user is not found. " +
                        "UserId={userId}", userId);
                    return NotFound();
                }

                _logger.LogInformation("UserController::GetUser: return user. " +
                    "Email={email} FirstName={firstName} AccountType={accountType}",
                    user.Email, user.FirstName, user.AccountType);
                return new JsonResult(user);
            }
            catch (Exception ex)
            {
                _logger.LogCritical("UserController::GetUser: exception raised. Msg: {exMsg}", ex.Message);
                return BadRequest("Exception raised");
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request)
        {
            var userId = HttpContext.Items["UserId"]!.ToString()!;
            _logger.LogInformation("UserController::UpdateUser: start update user information. " +
                "UserId={userId}, FirstName={firstName}, LastName={lastName}",
                userId, request.FirstName, request.LastName);

            if (string.IsNullOrWhiteSpace(request.FirstName))
            {
                _logger.LogWarning("UserController::UpdateUser: first name is empty or null. " +
                    "UserId={userId}", userId);
                return BadRequest("First name is required");
            }

            try
            {
                var user = await _authenticationService.UpdateUserAsync(
                    userId, 
                    request.FirstName.Trim(), 
                    request.LastName.Trim());
                
                if (user.AccountType == AppDomain.Models.Auth.AccountType.None)
                {
                    _logger.LogError("UserController::UpdateUser: user is not found. " +
                        "UserId={userId}", userId);
                    return NotFound();
                }

                _logger.LogInformation("UserController::UpdateUser: return updated user. " +
                    "Email={email} FirstName={firstName} LastName={lastName} AccountType={accountType}",
                    user.Email, user.FirstName, user.LastName, user.AccountType);
                return new JsonResult(user);
            }
            catch (Exception ex)
            {
                _logger.LogCritical("UserController::UpdateUser: exception raised. Msg: {exMsg}", ex.Message);
                return BadRequest("Exception raised");
            }
        }
    }
}
