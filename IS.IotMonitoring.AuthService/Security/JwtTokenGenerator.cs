using System.Security.Claims;
using System.Text;
using IS.IotMonitoring.AuthService.DTOs;
using IS.IotMonitoring.AuthService.Entities;
using IS.IotMonitoring.AuthService.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace IS.IotMonitoring.AuthService.Security
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        // Short claim names; CoreApi must use the same ones as NameClaimType / RoleClaimType.
        public const string NameClaimType = "name";
        public const string RoleClaimType = "role";

        private readonly JwtOptions options;
        private readonly SigningCredentials signingCredentials;
        private readonly JsonWebTokenHandler tokenHandler = new();

        public JwtTokenGenerator(IOptions<JwtOptions> options)
        {
            this.options = options.Value;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.options.SigningKey));
            signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }

        public TokenResponse Generate(User user)
        {
            var now = DateTimeOffset.UtcNow;
            var expiresAt = now.AddMinutes(options.ExpirationMinutes);

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = options.Issuer,
                Audience = options.Audience,
                IssuedAt = now.UtcDateTime,
                NotBefore = now.UtcDateTime,
                Expires = expiresAt.UtcDateTime,
                SigningCredentials = signingCredentials,
                Subject = new ClaimsIdentity(
                [
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(NameClaimType, user.Username),
                    new Claim(RoleClaimType, user.Role.ToString())
                ])
            };

            return new TokenResponse(tokenHandler.CreateToken(descriptor), expiresAt);
        }
    }
}
