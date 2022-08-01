using FinancialChat.Core.Contracts;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FinancialChat.Core.Helpers.Jwt;
using FinancialChat.Jwt.Helpers;

namespace FinancialChat.Jwt.Managers
{
    public sealed class JwtAuthManager : IJwtAuthManager
    {
        // Note: Refresh tokens could also be stored in a database or a distributed cache
        private readonly ConcurrentDictionary<string, RefreshToken> _usersRefreshTokens;

        private readonly JwtTokenConfig _jwtTokenConfig;
        private readonly byte[] _secret;

        public JwtAuthManager(JwtTokenConfig jwtTokenConfig)
        {
            _jwtTokenConfig = jwtTokenConfig;
            _usersRefreshTokens = new ConcurrentDictionary<string, RefreshToken>();
            _secret = Encoding.ASCII.GetBytes(jwtTokenConfig.Secret);
        }

        public JwtAuthResult GenerateTokens(string username, Claim[] claims)
        {
            bool shouldAddAudienceClaim =
                string.IsNullOrWhiteSpace(claims?.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Aud)?.Value);

            JwtSecurityToken jwtToken = new(
                _jwtTokenConfig.Issuer,
                shouldAddAudienceClaim ? _jwtTokenConfig.Audience : string.Empty,
                claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtTokenConfig.AccessTokenExpiration),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(_secret),
                    SecurityAlgorithms.HmacSha256Signature));

            string accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            RefreshToken refreshToken = new()
            {
                UserName = username,
                TokenString = GenerateRefreshTokenString(),
                ExpireAt = DateTime.UtcNow.AddMinutes(_jwtTokenConfig.RefreshTokenExpiration)
            };

            _usersRefreshTokens.AddOrUpdate(refreshToken.TokenString, refreshToken, (_, _) => refreshToken);

            return new JwtAuthResult
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public JwtAuthResult Refresh(string refreshToken, string accessToken)
        {
            var (principal, jwtToken) = DecodeJwtToken(accessToken);

            if (jwtToken is null || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature))
            {
                throw new SecurityTokenException("Invalid token");
            }

            string userName = principal.Identity?.Name;

            if (!_usersRefreshTokens.TryGetValue(refreshToken, out var existingRefreshToken))
            {
                throw new SecurityTokenException("Invalid token");
            }

            if (existingRefreshToken.UserName != userName || existingRefreshToken.ExpireAt < DateTime.UtcNow)
            {
                throw new SecurityTokenException("Invalid token");
            }

            return GenerateTokens(userName, principal.Claims.ToArray());
        }

        private (ClaimsPrincipal, JwtSecurityToken) DecodeJwtToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new SecurityTokenException("Invalid token");
            }

            ClaimsPrincipal principal = new JwtSecurityTokenHandler()
                .ValidateToken(token,
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = _jwtTokenConfig.Issuer,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(_secret),
                        ValidAudience = _jwtTokenConfig.Audience,
                        ValidIssuers = new List<string> { _jwtTokenConfig.Issuer },
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(1)
                    },
                    out var validatedToken);

            return (principal, validatedToken as JwtSecurityToken);
        }

        private string GenerateRefreshTokenString()
        {
            var randomNumber = new byte[32];
            using var randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public void RemoveRefreshTokenByUserName(string userName)
        {
            var refreshTokens = _usersRefreshTokens.Where(x => x.Value.UserName == userName).ToList();

            foreach (var refreshToken in refreshTokens)
            {
                _usersRefreshTokens.TryRemove(refreshToken.Key, out _);
            }
        }
    }
}
