using System;

namespace FinancialChat.Persistence.Models
{
    public sealed class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public bool Status { get; set; }
    }
}