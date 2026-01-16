using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Articler.WebApi.Services.Auth
{
    public class JwtSecurityService : IJwtSecurityService
    {
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;
        private readonly string _jwtKey;
        private readonly int _jwtExpiresDays;

        private readonly ILogger<IJwtSecurityService> _logger;

        private static SymmetricSecurityKey GetSymmetricSecurityKey(string key) => new(Encoding.UTF8.GetBytes(key));

        public JwtSecurityService(ILogger<IJwtSecurityService> logger, IConfiguration configuration)
        {
            _logger = logger;

            _jwtIssuer = configuration["Auth:Jwt:ValidIssuer"] ?? string.Empty;
            _jwtAudience = configuration["Auth:Jwt:ValidAudience"] ?? string.Empty;
            _jwtKey = configuration["Auth:Jwt:Key"] ?? string.Empty;

            if (int.TryParse(configuration["Auth:Jwt:ExpiresDays"], out var expiresDays))
            {
                _jwtExpiresDays = expiresDays;
            }
            else
            {
                _jwtExpiresDays = 0;
            }
        }

        public string CreateJwtToken(IList<Claim> claims)
        {
            _logger.LogInformation("JwtSecurityService::CreateJwtToken: begin create JWT token");

            var jwt = new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromDays(_jwtExpiresDays)),
                signingCredentials: new SigningCredentials(
                    GetSymmetricSecurityKey(_jwtKey),
                    SecurityAlgorithms.HmacSha256
                )
            );


            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
