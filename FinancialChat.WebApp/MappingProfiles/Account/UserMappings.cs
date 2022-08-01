using AutoMapper;
using FinancialChat.Persistence.Models;
using FinancialChat.WebApp.ViewModels.Account;

namespace FinancialChat.WebApp.MappingProfiles.Account
{
    public class UserMappings : Profile
    {
        public UserMappings()
        {
            CreateMap<User, UserViewModel>().ReverseMap();
        }
    }
}
