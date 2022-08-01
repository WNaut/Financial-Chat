using AutoMapper;
using FinancialChat.Core.Constants;
using FinancialChat.Core.Contracts;
using FinancialChat.Core.Helpers.Jwt;
using FinancialChat.Jwt.Constants;
using FinancialChat.Jwt.Helpers.Security;
using FinancialChat.Persistence.Models;
using FinancialChat.WebApp.Helpers;
using FinancialChat.WebApp.ViewModels.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FinancialChat.WebApp.Controllers
{
    public class AccountController : CustomController
    {
        private readonly IGenericRepository<User> _usersRepository;
        private readonly IJwtAuthManager _jwtAuthManager;
        private readonly IMapper _mapper;

        public AccountController(IGenericRepository<User> usersRepository,
            IJwtAuthManager jwtAuthManager, IMapper mapper)
        {
            _usersRepository = usersRepository;
            _jwtAuthManager = jwtAuthManager;
            _mapper = mapper;
        }
        
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (model is null)
            {
                return CustomBadRequest("Provided information is not valid.");
            }

            User user = await _usersRepository.FindAsync(user => user.Username == model.Username);

            if (user is null)
            {
                return CustomBadRequest("Invalid credentials.");
            }

            if (!user.Status)
            {
                return CustomBadRequest("User is inactive.");
            }

            bool isPasswordValid = PasswordHelper.CheckPassword(user.Password, model.Password);

            if (!isPasswordValid)
            {
                return CustomBadRequest("Invalid credentials.");
            }

            List<Claim> claims = new()
            {
                new Claim(FinancialChatClaimTypes.UserId, user.Id.ToString()),
                new Claim(ClaimTypes.Name, model.Username)
            };

            JwtAuthResult jwtResult = _jwtAuthManager.GenerateTokens(model.Username, claims.ToArray());

            TokenResponseViewModel tokenResponse = new()
            {
                UserId = user.Id,
                Username = model.Username,
                AccessToken = jwtResult.AccessToken,
                RefreshToken = jwtResult.RefreshToken.TokenString
            };

            return CustomOk(tokenResponse);
        }
        
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UserViewModel model)
        {
            if (model is null)
            {
                return CustomBadRequest("Provided information is not valid.");
            }

            User foundUser = await _usersRepository.FindAsync(u => u.Username == model.Username);

            if (foundUser is not null)
            {
                return CustomBadRequest("A user already exists with that username.");
            }

            User user = _mapper.Map<User>(model);
            user.Password = model.Password.HashPassword();
            user.CreationDate = DateTimeOffset.UtcNow;

            await _usersRepository.CreateAsync(user);
            
            return Ok();
        }
        
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            string userName = User.Identity?.Name;
            _jwtAuthManager.RemoveRefreshTokenByUserName(userName);

            return Ok();
        }
        
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.RefreshToken))
                {
                    return Unauthorized();
                }

                string accessToken = await HttpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, JwtConstants.TokenName);
                JwtAuthResult jwtResult = _jwtAuthManager.Refresh(request.RefreshToken, accessToken);

                Guid userId = Guid.Parse(User.FindFirstValue(FinancialChatClaimTypes.UserId));

                TokenResponseViewModel tokenResponse = new()
                {
                    UserId = userId,
                    Username = User.FindFirstValue(ClaimTypes.Name),
                    AccessToken = jwtResult.AccessToken,
                    RefreshToken = jwtResult.RefreshToken.TokenString
                };

                return CustomOk(tokenResponse);
            }
            catch (SecurityTokenException e)
            {
                return Unauthorized(e.Message);
            }
        }
    }
}