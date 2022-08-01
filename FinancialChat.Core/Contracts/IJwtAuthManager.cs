using FinancialChat.Core.Helpers.Jwt;
using System.Security.Claims;

namespace FinancialChat.Core.Contracts
{
    public interface IJwtAuthManager
    {
        JwtAuthResult GenerateTokens(string username, Claim[] claims);
        JwtAuthResult Refresh(string refreshToken, string accessToken);
        void RemoveRefreshTokenByUserName(string userName);
    }
}
