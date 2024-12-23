using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CafeManagementSystem.Models;

namespace CafeManagementSystem
{
    public class TokenManager
    {
        public static string Secret = "WProjekcieNieDodaliFrontenduIBedeGoPrzepisywacZJavyISpringBoota";

        public static string GenerateToken(string email, string role)
        {
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                    { new Claim(ClaimTypes.Email, email), new Claim(ClaimTypes.Role, role) }),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
            };
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = handler.CreateJwtSecurityToken(descriptor);

            return handler.WriteToken(token);
        }

        // validate received token and return user identity
        public static ClaimsPrincipal GetPrincipal(string token)
        {
            try
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtToken = (JwtSecurityToken)tokenHandler.ReadToken(token); // parsing read token
                if (jwtToken == null)
                    return null;

                // setting parameters - expiration time, sender and receiver validation if off, verify token with Secret
                TokenValidationParameters parameters = new TokenValidationParameters()
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret))
                };

                SecurityToken securityToken;
                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, parameters, out securityToken);
                return principal; // returning ClaimsPrincipal object representing user
            }
            catch (Exception e)
            {
                return null;
            }
        }

        // claiming email and role from token, cutting of "Bearer".
        public static TokenClaim ValidateToken(string rawToken)
        {
            string[] array = rawToken.Split(' '); // splitting to parts, to separate "Bearer" prefix [0]
            var token = array[1]; // Bearer is at [0], so we're getting rid of it
            ClaimsPrincipal principal = GetPrincipal(token);
            if (principal == null)
                return null;

            ClaimsIdentity identity = null;
            try
            {
                identity = (ClaimsIdentity)principal.Identity; // taking identity form principal object
            }
            catch (Exception e)
            {
                return null;
            }

            // creating "TokenClaim" object that handle email and role for specific user
            TokenClaim tokenClaim = new TokenClaim();
            var temp = identity.FindFirst(ClaimTypes.Email);
            tokenClaim.Email = temp.Value;
            temp = identity.FindFirst(ClaimTypes.Role);
            tokenClaim.Role = temp.Value;
            return tokenClaim;
        }
    }
}