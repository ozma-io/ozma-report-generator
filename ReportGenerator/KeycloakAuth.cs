using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ReportGenerator
{
    public class KeycloakAuth
    {
        private IConfiguration configuration { get; }
        public KeycloakAuth(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string? GetToken(ClaimsIdentity? identity)
        {
            if (identity == null) return null;

            var now = DateTime.Now;
            var jwt = new JwtSecurityToken(
                issuer: configuration["KeycloakAuth:Issuer"],
                audience: configuration["KeycloakAuth:Audience"],
                notBefore: now,
                claims: identity.Claims,
                expires: now.Add(TimeSpan.FromMinutes(30)),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["KeycloakAuth:key"])),
                    SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            return encodedJwt;
        }

        public ClaimsIdentity? GetUserIdentity(string userName)
        {
            if (userName != "Administrator") return null;

            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, "Administrator")
            };
            ClaimsIdentity claimsIdentity =
                new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType);
            return claimsIdentity;
        }
    }
}
