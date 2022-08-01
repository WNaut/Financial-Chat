using System;

namespace FinancialChat.WebApp.ViewModels.Account
{
    public class TokenResponseViewModel
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
